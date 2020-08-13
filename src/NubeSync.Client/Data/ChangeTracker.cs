using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NubeSync.Client.Data
{
    public class ChangeTracker : IChangeTracker
    {
        public Task<List<NubeOperation>> TrackAddAsync<T>(T item) where T : NubeTable
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentNullException("item id");
            }

            var tableName = item.GetType().Name;
            var operations = new List<NubeOperation>
            {
                new NubeOperation()
                {
                    TableName = tableName,
                    ItemId = item.Id,
                    Type = OperationType.Added
                }
            };

            foreach (var property in item.GetProperties())
            {
                if (property.Value != default)
                {
                    operations.Add(new NubeOperation()
                    {
                        TableName = tableName,
                        ItemId = item.Id,
                        Type = OperationType.Modified,
                        Property = property.Key,
                        Value = property.Value
                    });
                }
            }

            return Task.FromResult(operations);
        }

        public Task<NubeOperation> TrackDeleteAsync<T>(T item) where T : NubeTable
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentNullException("item id");
            }

            var operation = new NubeOperation()
            {
                TableName = item.GetType().Name,
                ItemId = item.Id,
                Type = OperationType.Deleted
            };

            return Task.FromResult(operation);
        }

        public Task<List<NubeOperation>> TrackModifyAsync<T>(T oldItem, T newItem) where T : NubeTable
        {
            if (string.IsNullOrEmpty(newItem.Id))
            {
                throw new ArgumentNullException("newItem id");
            }

            if (oldItem.GetType() != newItem.GetType())
            {
                throw new InvalidOperationException("Cannot compare objects of different types");
            }

            if (oldItem.Id != newItem.Id)
            {
                throw new InvalidOperationException("Cannot compare different records");
            }

            var oldProperties = oldItem.GetProperties();
            var newProperties = newItem.GetProperties();
            var tableName = newItem.GetType().Name;
            var operations = new List<NubeOperation>();

            foreach (var property in newProperties)
            {
                var oldPropertyValue = oldProperties[property.Key];
                if (oldPropertyValue != property.Value)
                {
                    operations.Add(new NubeOperation()
                    {
                        TableName = tableName,
                        ItemId = newItem.Id,
                        Type = OperationType.Modified,
                        Property = property.Key,
                        Value = property.Value,
                        OldValue = oldPropertyValue
                    });
                }
            }

            return Task.FromResult(operations);
        }
    }
}