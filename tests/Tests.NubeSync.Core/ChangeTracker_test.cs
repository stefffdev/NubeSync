using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NubeSync.Core;
using Xunit;

namespace Tests.NubeSync.Core.ChangeTracker_test
{
    public class _ChangeTrackerTestBase
    {
        internal ChangeTracker _changeTracker;
        protected TestItem Item;

        public _ChangeTrackerTestBase()
        {
            _changeTracker = new ChangeTracker();
            Item = TestFactory.CreateTestItem("ExpectedId", "ExpectedName", DateTimeOffset.Now);
        }
    }

    public class Track_add : _ChangeTrackerTestBase
    {
        [Fact]
        public async Task Creates_add_before_modify()
        {
            var operations = await _changeTracker.TrackAddAsync(Item);

            var addCreate = operations.Where(o => o.Type == OperationType.Added).Max(o => o.CreatedAt);
            var modifyCreate = operations.Where(o => o.Type == OperationType.Modified).Min(o => o.CreatedAt);
            Assert.True(addCreate < modifyCreate);
        }

        [Fact]
        public async Task Creates_the_add_operation()
        {
            var operations = await _changeTracker.TrackAddAsync(Item);

            var addOperations = operations.Where(o => o.Type == OperationType.Added);
            var addOperation = addOperations.FirstOrDefault();
            Assert.Single(addOperations);
            Assert.DoesNotContain(operations, o => o.Type == OperationType.Deleted);
            Assert.Equal("TestItem", addOperation.TableName);
            Assert.Equal(Item.Id, addOperation.ItemId);
            Assert.Equal(OperationType.Added, addOperation.Type);
        }

        [Fact]
        public async Task Creates_the_modify_operations()
        {
            var operations = await _changeTracker.TrackAddAsync(Item);

            var modifyOperations = operations.Where(o => o.Type == OperationType.Modified);
            Assert.Equal(3, modifyOperations.Count());
            Assert.Equal(3, modifyOperations.Where(o => o.ItemId == Item.Id).Count());
            Assert.Equal(3, modifyOperations.Where(o => o.TableName == "TestItem").Count());

            var nameItem = modifyOperations.Where(o => o.Property == "Name").First();
            Assert.Null(nameItem.OldValue);
            Assert.Equal(Item.Name, nameItem.Value);

            var createdAtItem = modifyOperations.Where(o => o.Property == "CreatedAt").First();
            Assert.Null(createdAtItem.OldValue);
            Assert.Equal(Item.CreatedAt.ToString("o", CultureInfo.InvariantCulture), createdAtItem.Value);

            var updatedAtItem = modifyOperations.Where(o => o.Property == "UpdatedAt").First();
            Assert.Null(updatedAtItem.OldValue);
            Assert.Equal(Item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture), updatedAtItem.Value);
        }

        [Fact]
        public async Task Throws_when_item_id_is_empty()
        {
            Item.Id = string.Empty;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackAddAsync(Item));
            Assert.Equal("Value cannot be null. (Parameter 'item id')", ex.Message);
        }

        [Fact]
        public async Task Throws_when_item_id_is_null()
        {
            Item.Id = null;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackAddAsync(Item));
            Assert.Equal("Value cannot be null. (Parameter 'item id')", ex.Message);
        }
    }

    public class Track_delete : _ChangeTrackerTestBase
    {
        [Fact]
        public async Task Creates_the_delete_operation()
        {
            var operation = (await _changeTracker.TrackDeleteAsync(Item)).First();

            Assert.Equal("TestItem", operation.TableName);
            Assert.Equal(Item.Id, operation.ItemId);
            Assert.Equal(OperationType.Deleted, operation.Type);
        }

        [Fact]
        public async Task Throws_when_item_id_is_empty()
        {
            Item.Id = string.Empty;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackDeleteAsync(Item));
            Assert.Equal("Value cannot be null. (Parameter 'item id')", ex.Message);
        }

        [Fact]
        public async Task Throws_when_item_id_is_null()
        {
            Item.Id = null;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackDeleteAsync(Item));
            Assert.Equal("Value cannot be null. (Parameter 'item id')", ex.Message);
        }
    }

    public class Track_modify : _ChangeTrackerTestBase
    {
        private readonly TestItem _newItem;

        public Track_modify()
        {
            _newItem = TestFactory.CreateTestItem(Item.Id, "NewName", DateTimeOffset.Now.AddSeconds(1));
        }

        [Fact]
        public async Task Creates_operations_for_all_changed_fields()
        {
            var operations = await _changeTracker.TrackModifyAsync(Item, _newItem);

            Assert.Equal(3, operations.Count);
            Assert.Equal(3, operations.Where(o => o.Type == OperationType.Modified).Count());
            Assert.Equal(3, operations.Where(o => o.ItemId == Item.Id).Count());
            Assert.Equal(3, operations.Where(o => o.TableName == "TestItem").Count());
            var nameOperation = operations.Where(o => o.Property == "Name").First();
            Assert.Equal(Item.Name, nameOperation.OldValue);
            Assert.Equal(_newItem.Name, nameOperation.Value);
            var updatedAtOperation = operations.Where(o => o.Property == "UpdatedAt").First();
            Assert.Equal(Item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture), updatedAtOperation.OldValue);
            Assert.Equal(_newItem.UpdatedAt.ToString("o", CultureInfo.InvariantCulture), updatedAtOperation.Value);
            var createdAtOperation = operations.Where(o => o.Property == "CreatedAt").First();
            Assert.Equal(Item.CreatedAt.ToString("o", CultureInfo.InvariantCulture), createdAtOperation.OldValue);
            Assert.Equal(_newItem.CreatedAt.ToString("o", CultureInfo.InvariantCulture), createdAtOperation.Value);
        }

        [Fact]
        public async Task Does_nothing_when_no_properties_changed()
        {
            var operations = await _changeTracker.TrackModifyAsync(Item, Item);

            Assert.Empty(operations);
        }

        [Fact]
        public async Task Throws_when_items_types_are_different()
        {
            var newItem = new TestItem2() { Id = "test" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _changeTracker.TrackModifyAsync<NubeTable>(Item, newItem));
            Assert.Equal("Cannot compare objects of different types", ex.Message);
        }

        [Fact]
        public async Task Throws_when_new_item_id_is_empty()
        {
            _newItem.Id = string.Empty;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackModifyAsync(Item, _newItem));
            Assert.Equal("Value cannot be null. (Parameter 'newItem id')", ex.Message);
        }

        [Fact]
        public async Task Throws_when_new_item_id_is_null()
        {
            _newItem.Id = null;

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _changeTracker.TrackModifyAsync(Item, _newItem));
            Assert.Equal("Value cannot be null. (Parameter 'newItem id')", ex.Message);
        }

        [Fact]
        public async Task Throws_when_the_ids_are_different()
        {
            _newItem.Id = "new";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _changeTracker.TrackModifyAsync<NubeTable>(Item, _newItem));
            Assert.Equal("Cannot compare different records", ex.Message);
        }
    }
}