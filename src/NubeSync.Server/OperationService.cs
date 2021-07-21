using Microsoft.EntityFrameworkCore;
using NubeSync.Core;
using NubeSync.Server.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace NubeSync.Server
{
    public class OperationService : IOperationService
    {
        private readonly Dictionary<string, Tuple<Type, Func<object>>> _nubeTableTypes =
            new Dictionary<string, Tuple<Type, Func<object>>>();

        public OperationService(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTable(type);
            }
        }

        public bool LastChangedByOthers(
            DbContext context,
            string tableName,
            string itemId,
            string installationId,
            DateTimeOffset laterThan)
        {
            if (string.IsNullOrEmpty(installationId))
            {
                return true;
            }

            return context.Set<NubeServerOperation>().Where(
                o => o.ItemId == itemId && o.TableName == tableName &&
                o.ServerUpdatedAt >= laterThan &&
                o.ProcessingType == ProcessingType.Processed &&
                o.InstallationId != installationId).Any();
        }

        public async Task<List<(NubeServerTable Entity, OperationType Type)>> ProcessOperationsAsync(
            DbContext context,
            IList<NubeOperation> operations,
            string userId = "",
            string installationId = "")
        {
            var result = new List<(NubeServerTable, OperationType)>();

            var serverOperations = operations
                .Where(o => _DoesNotExist(context, o))
                .Select(x => new NubeServerOperation
                {
                    Id = x.Id,
                    CreatedAt = x.CreatedAt,
                    ItemId = x.ItemId,
                    OldValue = x.OldValue,
                    Property = x.Property,
                    TableName = x.TableName,
                    Type = x.Type,
                    Value = x.Value,
                    UserId = userId,
                    InstallationId = installationId
                }).ToList();

            var operationGroups = serverOperations.GroupBy(x => new { x.TableName, x.ItemId });

            foreach (var operationGroup in serverOperations.GroupBy(x => new { x.TableName, x.ItemId }))
            {
                var type = _GetTableType(operationGroup.Key.TableName);
                if (type == null)
                {
                    throw new NullReferenceException($"The type {operationGroup.Key.TableName} cannot be found");
                }

                var itemOperations = operationGroup.OrderBy(o => o.CreatedAt).ToArray();

                if (operationGroup.Any(o => o.Type == OperationType.Added))
                {
                    result.Add(await _ProcessAddAsync(context, itemOperations, type).ConfigureAwait(false));
                }
                else if (operationGroup.Any(o => o.Type == OperationType.Deleted))
                {
                    result.Add(await _ProcessDeleteAsync(context, itemOperations, type).ConfigureAwait(false));
                }
                else if (operationGroup.All(o => o.Type == OperationType.Modified) &&
                    itemOperations.Any())
                {
                    result.Add(await _ProcessModifyAsync(context, itemOperations, type).ConfigureAwait(false));
                }
                else
                {
                    throw new Exception("Unknown operation sequence");
                }

                var now = DateTimeOffset.Now;
                operationGroup.ToList().ForEach(o => o.ServerUpdatedAt = now);

                await context.AddRangeAsync(operationGroup.ToList()).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            return result;
        }

        public void RegisterTable(Type type)
        {
            _nubeTableTypes.Add(type.Name, Tuple.Create(type, Expression.Lambda<Func<object>>(Expression.New(type)).Compile()));
        }

        private bool _DoesNotExist(DbContext context, NubeOperation operation)
        {
            return context.Set<NubeServerOperation>().Find(operation.Id) == null;
        }

        private async Task<DateTimeOffset> _GetLastChangeForPropertyAsync(
            DbContext context,
            string itemId,
            string tableName,
            string propertyName)
        {
            return await context.Set<NubeServerOperation>()
                .AsNoTracking().Where(o =>
                    o.ItemId == itemId &&
                    o.TableName == tableName &&
                    o.Property == propertyName)
                .MaxAsync(o => (DateTimeOffset?) o.CreatedAt)
                .ConfigureAwait(false) ?? DateTimeOffset.MinValue;
        }

        private Type? _GetTableType(string tableName)
        {
            if (_nubeTableTypes.ContainsKey(tableName))
            {
                return _nubeTableTypes[tableName].Item1;
            }

            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => t.Name == tableName &&
                    t.BaseType == typeof(NubeServerTable));

            if (type != null)
            {
                RegisterTable(type);
            }

            return type;
        }

        private async Task<(NubeServerTable, OperationType)> _ProcessAddAsync(
            DbContext context,
            NubeServerOperation[] operations,
            Type type)
        {
            var addOperation = operations.Where(o => o.Type == OperationType.Added).First();

            var newItem = _nubeTableTypes[type.Name].Item2();
            if (newItem == null)
            {
                throw new NullReferenceException($"Item of type {type} cannot be created");
            }

            if (newItem is NubeServerTable entity)
            {
                entity.Id = addOperation.ItemId;
                entity.UserId = addOperation.UserId;
                entity.ServerUpdatedAt = DateTimeOffset.Now;

                foreach (var operation in operations.Where(o => o.Type == OperationType.Modified))
                {
                    _UpdatePropertyFromOperation(newItem, operation, type);
                }

                await context.AddAsync(newItem).ConfigureAwait(false);

                return (entity, OperationType.Added);
            }
            else
            {
                throw new InvalidOperationException("Created item is not of type NubeServerTable");
            }
        }

        private async Task<(NubeServerTable, OperationType)> _ProcessDeleteAsync(
            DbContext context,
            NubeServerOperation[] operations,
            Type type)
        {
            var deleteOperation = operations.Where(o => o.Type == OperationType.Deleted).First();

            var item = await context.FindAsync(type, deleteOperation.ItemId).ConfigureAwait(false);
            if (item is NubeServerTable localItem)
            {
                var now = DateTimeOffset.Now;
                localItem.DeletedAt = now;
                localItem.ServerUpdatedAt = now;

                return (localItem, OperationType.Deleted);
            }
            else
            {
                throw new InvalidOperationException("Deleted item is not of type NubeServerTable");
            }
        }

        private async Task<(NubeServerTable, OperationType)> _ProcessModifyAsync(
            DbContext context,
            NubeServerOperation[] operations,
            Type type)
        {
            var item = await context.FindAsync(type, operations[0].ItemId).ConfigureAwait(false);
            if (item is NubeServerTable localItem)
            {
                foreach (var operation in operations)
                {
                    if (localItem.DeletedAt.HasValue)
                    {
                        operation.ProcessingType = ProcessingType.DiscardedDeleted;
                    }
                    else
                    {
                        if (operation.Property == null)
                        {
                            throw new InvalidOperationException($"Property of operation {operation.Id} cannot be empty");
                        }

                        if (localItem.UpdatedAt >= operation.CreatedAt &&
                            await _GetLastChangeForPropertyAsync(context, operation.ItemId, operation.TableName, operation.Property).ConfigureAwait(false) > operation.CreatedAt)
                        {
                            operation.ProcessingType = ProcessingType.DiscaredOutdated;
                        }
                        else
                        {
                            _UpdatePropertyFromOperation(item, operation, type);
                        }
                    }
                }

                return (localItem, OperationType.Modified);
            }
            else
            {
                throw new InvalidOperationException($"Item with the id {operations[0].ItemId} cannot be found");
            }
        }

        private void _UpdatePropertyFromOperation(
            object item,
            NubeServerOperation operation,
            Type type)
        {
            if (string.IsNullOrEmpty(operation.Property))
            {
                throw new InvalidOperationException($"Property name cannot be empty");
            }

            var prop = type.GetProperty(operation.Property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new InvalidOperationException($"Property {operation.Property} not found on item {type}");
            }

            var converter = TypeDescriptor.GetConverter(prop.PropertyType);
            if (!converter.CanConvertFrom(typeof(string)))
            {
                throw new InvalidOperationException($"Unable to convert value {operation.Property} of operation {operation.Id}");
            }

            try
            {
                var val = operation.Value == null ? operation.Value :
                    converter.ConvertFromInvariantString(operation.Value);

                prop.SetValue(item, val);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to convert value {operation.Property} of operation {operation.Id}: {ex.Message}");
            }

            if (item is NubeServerTable localItem)
            {
                localItem.ServerUpdatedAt = DateTimeOffset.Now;
            }
        }
    }
}