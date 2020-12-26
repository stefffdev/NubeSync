using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NubeSync.Core;

namespace NubeSync.Client
{
    public partial class NubeClient
    {
        public async Task DeleteAsync<T>(T item, bool disableChangeTracker = false) where T : NubeTable
        {
            _IsValidTable<T>();

            if (string.IsNullOrEmpty(item.Id))
            {
                throw new InvalidOperationException("Cannot delete item without id");
            }

            if (await _dataStore.DeleteAsync(item).ConfigureAwait(false))
            {
                if (!disableChangeTracker)
                {
                    await _SaveDeleteOperations(item).ConfigureAwait(false);
                    await _RemoveObsoleteOperationsAfterDeleteAsync(item).ConfigureAwait(false);
                }
            }
            else
            {
                throw new StoreOperationFailedException($"Could not delete item");
            }
        }

        public async Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable
        {
            _IsValidTable<T>();

            return await _dataStore.FindByAsync(predicate).ConfigureAwait(false);
        }

        public async Task<IQueryable<T>> GetAllAsync<T>() where T : NubeTable
        {
            _IsValidTable<T>();

            return await _dataStore.AllAsync<T>().ConfigureAwait(false);
        }

        public async Task<T> GetByIdAsync<T>(string id) where T : NubeTable
        {
            _IsValidTable<T>();

            return await _dataStore.FindByIdAsync<T>(id).ConfigureAwait(false);
        }

        public async Task SaveAsync<T>(T item, T? existingItem = null, bool disableChangeTracker = false) where T : NubeTable
        {
            _IsValidTable<T>();

            var now = DateTimeOffset.Now;
            if (!disableChangeTracker)
            {
                item.UpdatedAt = now;
            }

            if (existingItem == null && !string.IsNullOrEmpty(item.Id))
            {
                existingItem = await _dataStore.FindByIdAsync<T>(item.Id).ConfigureAwait(false);
            }

            if (existingItem == null)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = Guid.NewGuid().ToString();
                }

                if (!disableChangeTracker)
                {
                    item.CreatedAt = now;
                }

                if (await _dataStore.InsertAsync(item).ConfigureAwait(false))
                {
                    if (!disableChangeTracker)
                    {
                        await _SaveAddOperations(item).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new StoreOperationFailedException($"Could not insert item");
                }
            }
            else
            {
                if (await _dataStore.UpdateAsync(item).ConfigureAwait(false))
                {
                    if (!disableChangeTracker)
                    {
                        await _SaveModifyOperations(item, existingItem).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new StoreOperationFailedException($"Could not update item");
                }
            }
        }

        private void _IsValidTable<T>()
        {
            if (!_nubeTableTypes.ContainsKey(typeof(T).Name))
            {
                throw new InvalidOperationException($"Table {typeof(T).Name} is not registered in the nube client");
            }
        }

        private async Task _RemoveObsoleteOperationsAfterDeleteAsync<T>(T item) where T : NubeTable
        {
            var obsoleteOperations = (await _dataStore.GetOperationsAsync().ConfigureAwait(false)).Where(o => 
                o.ItemId == item.Id &&
                o.TableName == item.GetType().Name &&
                o.Type != OperationType.Deleted)
                .ToList();
            if (!await _dataStore.DeleteOperationsAsync(obsoleteOperations.ToArray()).ConfigureAwait(false))
            {
                throw new StoreOperationFailedException($"Could not delete obsolete operations for deleted item {item.Id}");
            }
        }

        private async Task _RemoveObsoleteOperationsAfterModifyAsync(List<NubeOperation> operations)
        {
            var obsoleteOperations = new List<NubeOperation>();
            foreach (var operation in operations)
            {
                var obsolete = (await _dataStore.GetOperationsAsync().ConfigureAwait(false)).Where(o => 
                    o.Id != operation.Id &&
                    o.ItemId == operation.ItemId && 
                    o.TableName == operation.TableName && 
                    o.Property == operation.Property && 
                    o.Type == OperationType.Modified);
                obsoleteOperations.AddRange(obsolete);
            }

            if (!await _dataStore.DeleteOperationsAsync(obsoleteOperations.ToArray()).ConfigureAwait(false))
            {
                throw new StoreOperationFailedException($"Could not delete obsolete operations for modified item");
            }
        }

        private async Task _SaveAddOperations<T>(T item) where T : NubeTable
        {
            var operations = await _changeTracker.TrackAddAsync(item).ConfigureAwait(false);
            if (!await _dataStore.AddOperationsAsync(operations.ToArray()).ConfigureAwait(false))
            {
                throw new StoreOperationFailedException($"Could not save add operations for item {item.Id}");
            }
        }

        private async Task _SaveDeleteOperations<T>(T item) where T : NubeTable
        {
            var operations = await _changeTracker.TrackDeleteAsync(item).ConfigureAwait(false);
            if (!await _dataStore.AddOperationsAsync(operations.ToArray()).ConfigureAwait(false))
            {
                throw new StoreOperationFailedException($"Could not save delete operation for item {item.Id}");
            }
        }

        private async Task _SaveModifyOperations<T>(T item, T oldItem) where T : NubeTable
        {
            var operations = await _changeTracker.TrackModifyAsync(oldItem, item).ConfigureAwait(false);
            if (!await _dataStore.AddOperationsAsync(operations.ToArray()).ConfigureAwait(false))
            {
                throw new StoreOperationFailedException($"Could not save modify operations for item {item.Id}");
            }

            await _RemoveObsoleteOperationsAfterModifyAsync(operations).ConfigureAwait(false);
        }
    }
}