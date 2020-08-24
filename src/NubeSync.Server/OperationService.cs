using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Core;
using NubeSync.Server.Data;

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

        public async Task ProcessOperationsAsync(
            DbContext context,
            IList<NubeOperation> operations,
            string userId = "",
            string installationId = "")
        {
            var serverOperations = operations.Select(x => new NubeServerOperation
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

            foreach (var operationGroup in serverOperations.GroupBy(x => new { x.TableName, x.ItemId }))
            {
                var type = _GetTableType(operationGroup.Key.TableName);
                if (type == null)
                {
                    throw new NullReferenceException($"The type {operationGroup.Key.TableName} cannot be found");
                }

                var added = operationGroup
                    .Where(o => o.Type == OperationType.Added)
                    .OrderBy(o => o.CreatedAt).ToArray();
                await _ProcessAddsAsync(context, added, type).ConfigureAwait(false);

                var modified = operationGroup
                    .Where(o => o.Type == OperationType.Modified)
                    .OrderBy(o => o.CreatedAt).ToArray();
                await _ProcessModifiesAsync(context, modified, type).ConfigureAwait(false);

                var deleted = operationGroup
                    .Where(o => o.Type == OperationType.Deleted)
                    .OrderBy(o => o.CreatedAt).ToArray();
                await _ProcessDeletesAsync(context, deleted, type).ConfigureAwait(false);

                operationGroup.ToList().ForEach(o => o.ServerUpdatedAt = DateTimeOffset.Now);
                await context.AddRangeAsync(operationGroup.ToList()).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public void RegisterTable(Type type)
        {
            _nubeTableTypes.Add(type.Name, Tuple.Create(type, Expression.Lambda<Func<object>>(Expression.New(type)).Compile()));
        }

        private async Task<DateTimeOffset> _GetLastChangeForPropertyAsync(
            DbContext context,
            string itemId,
            string propertyName)
        {
            return await context.Set<NubeServerOperation>().AsNoTracking()
                .Where(o => o.ItemId == itemId && o.Property == propertyName)
                .MaxAsync(o => (DateTimeOffset?) o.CreatedAt).ConfigureAwait(false) ?? DateTimeOffset.MinValue;
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
                .FirstOrDefault(t => t.Name == tableName);

            if (type != null)
            {
                RegisterTable(type);
            }

            return type;
        }

        private async Task _ProcessAddsAsync(DbContext context, NubeServerOperation[] operations, Type type)
        {
            foreach (var operation in operations)
            {
                var newItem = _nubeTableTypes[type.Name].Item2();
                if (newItem == null)
                {
                    throw new NullReferenceException($"Item of type {type} cannot be created");
                }

                if (newItem is NubeServerTable entity)
                {
                    entity.Id = operation.ItemId;
                    entity.UserId = operation.UserId;
                    entity.ServerUpdatedAt = DateTimeOffset.Now;
                }

                await context.AddAsync(newItem).ConfigureAwait(false);
            }
        }

        private async Task _ProcessDeletesAsync(DbContext context, NubeServerOperation[] operations, Type type)
        {
            foreach (var operation in operations)
            {
                var item = await context.FindAsync(type, operation.ItemId).ConfigureAwait(false);
                if (item is NubeServerTable localItem)
                {
                    var now = DateTimeOffset.Now;
                    localItem.DeletedAt = now;
                    localItem.ServerUpdatedAt = now;
                }
            }
        }

        private async Task _ProcessModifiesAsync(DbContext context, NubeServerOperation[] operations, Type type)
        {
            if (!operations.Any())
            {
                return;
            }

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
                        if (string.IsNullOrEmpty(operation.Property))
                        {
                            throw new InvalidOperationException($"Property of operation {operation.Id} cannot be empty");
                        }

                        if (await _GetLastChangeForPropertyAsync(context, operation.ItemId, operation.Property).ConfigureAwait(false) > operation.CreatedAt)
                        {
                            operation.ProcessingType = ProcessingType.DiscaredOutdated;
                        }
                        else
                        {
                            var prop = type.GetProperty(operation.Property,
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop == null)
                            {
                                throw new InvalidOperationException($"Property {operation.Property} not found on item {type}");
                            }

                            var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                            if (!converter.CanConvertFrom(typeof(string)))
                            {
                                throw new InvalidOperationException($"Unable to convert value of operation {operation.Id}");
                            }

                            prop.SetValue(item, converter.ConvertFromInvariantString(operation.Value));
                            localItem.ServerUpdatedAt = DateTimeOffset.Now;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Item with the id {operations[0].ItemId} cannot be found");
            }
        }
    }
}