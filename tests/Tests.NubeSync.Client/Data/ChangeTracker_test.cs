using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NubeSync.Client.Data;
using Xunit;

namespace Tests.NubeSync.Client.Data.ChangeTracker_test
{
    public class _ChangeTrackerTestBase
    {
        internal ChangeTracker _changeTracker;
        protected List<NubeOperation> AddedOperations;
        protected IDataStore DataStore;
        protected TestItem Item;
        protected List<NubeOperation> RemovedOperations;

        public _ChangeTrackerTestBase()
        {
            AddedOperations = new List<NubeOperation>();
            RemovedOperations = new List<NubeOperation>();

            DataStore = TestFactory.CreateDataStore();
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(true);
            DataStore.When(x => x.AddOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                y => AddedOperations.AddRange(y.Arg<NubeOperation[]>()));
            DataStore.When(x => x.DeleteOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                y => RemovedOperations.AddRange(y.Arg<NubeOperation[]>()));

            _changeTracker = new ChangeTracker(DataStore);

            Item = TestFactory.CreateTestItem("ExpectedId", "ExpectedName", DateTimeOffset.Now);
        }
    }

    public class Track_add : _ChangeTrackerTestBase
    {
        [Fact]
        public async Task Creates_add_before_modify()
        {
            await _changeTracker.TrackAddAsync(Item);

            var addCreate = AddedOperations.Where(o => o.Type == OperationType.Added).Max(o => o.CreatedAt);
            var modifyCreate = AddedOperations.Where(o => o.Type == OperationType.Modified).Min(o => o.CreatedAt);
            Assert.True(addCreate < modifyCreate);
        }

        [Fact]
        public async Task Creates_the_add_operation()
        {
            await _changeTracker.TrackAddAsync(Item);

            var addOperations = AddedOperations.Where(o => o.Type == OperationType.Added);
            var addOperation = addOperations.FirstOrDefault();
            Assert.Single(addOperations);
            Assert.DoesNotContain(AddedOperations, o => o.Type == OperationType.Deleted);
            Assert.Equal("TestItem", addOperation.TableName);
            Assert.Equal(Item.Id, addOperation.ItemId);
            Assert.Equal(OperationType.Added, addOperation.Type);
        }

        [Fact]
        public async Task Creates_the_modify_operations()
        {
            await _changeTracker.TrackAddAsync(Item);

            var modifyOperations = AddedOperations.Where(o => o.Type == OperationType.Modified);
            Assert.Equal(3, modifyOperations.Count());
            Assert.Equal(3, modifyOperations.Where(o => o.ItemId == Item.Id).Count());
            Assert.Equal(3, modifyOperations.Where(o => o.TableName == "TestItem").Count());

            var nameItem = modifyOperations.Where(o => o.Property == "Name").First();
            Assert.Null(nameItem.OldValue);
            Assert.Equal(Item.Name, nameItem.Value);

            var createdAtItem = modifyOperations.Where(o => o.Property == "CreatedAt").First();
            Assert.Null(createdAtItem.OldValue);
            Assert.Equal(Convert.ToString(Item.CreatedAt, CultureInfo.InvariantCulture), createdAtItem.Value);

            var updatedAtItem = modifyOperations.Where(o => o.Property == "UpdatedAt").First();
            Assert.Null(updatedAtItem.OldValue);
            Assert.Equal(Convert.ToString(Item.UpdatedAt, CultureInfo.InvariantCulture), updatedAtItem.Value);
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

        [Fact]
        public async Task Throws_when_operations_cannot_be_stored()
        {
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await _changeTracker.TrackAddAsync(Item));

            Assert.Equal($"Could not save add operations for item {Item.Id}", ex.Message);
        }
    }

    public class Track_delete : _ChangeTrackerTestBase
    {
        [Fact]
        public async Task Cleans_up_obsolete_operations()
        {
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Deleted },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Added },
            };
            var expectedOperations = existingOperations.Skip(2).ToList();
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());

            await _changeTracker.TrackDeleteAsync(Item);

            Assert.Equal(expectedOperations, RemovedOperations);
        }

        [Fact]
        public async Task Creates_the_delete_operation()
        {
            await _changeTracker.TrackDeleteAsync(Item);

            var deletedOperations = AddedOperations.Where(o => o.Type == OperationType.Deleted);
            var deleteOperation = deletedOperations.FirstOrDefault();
            Assert.Single(deletedOperations);
            Assert.DoesNotContain(AddedOperations, o => o.Type == OperationType.Modified || o.Type == OperationType.Added);
            Assert.Equal("TestItem", deleteOperation.TableName);
            Assert.Equal(Item.Id, deleteOperation.ItemId);
            Assert.Equal(OperationType.Deleted, deleteOperation.Type);
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

        [Fact]
        public async Task Throws_when_operations_cannot_be_stored()
        {
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await _changeTracker.TrackDeleteAsync(Item));

            Assert.Equal($"Could not save delete operation for item {Item.Id}", ex.Message);
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
        public async Task Cleans_up_obsolete_operations()
        {
            _newItem.CreatedAt = Item.CreatedAt;
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Property = "CreatedAt", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Property = "Name", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Property = "UpdatedAt", Type = OperationType.Modified },
            };
            var expectedOperations = existingOperations.Skip(2).ToList();
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());

            await _changeTracker.TrackModifyAsync(Item, _newItem);

            Assert.Equal(expectedOperations, RemovedOperations);
        }

        [Fact]
        public async Task Creates_operations_for_all_changed_fields()
        {
            await _changeTracker.TrackModifyAsync(Item, _newItem);

            Assert.Equal(3, AddedOperations.Count);
            Assert.Equal(3, AddedOperations.Where(o => o.Type == OperationType.Modified).Count());
            Assert.Equal(3, AddedOperations.Where(o => o.ItemId == Item.Id).Count());
            Assert.Equal(3, AddedOperations.Where(o => o.TableName == "TestItem").Count());
            var nameOperation = AddedOperations.Where(o => o.Property == "Name").First();
            Assert.Equal(Item.Name, nameOperation.OldValue);
            Assert.Equal(_newItem.Name, nameOperation.Value);
            var updatedAtOperation = AddedOperations.Where(o => o.Property == "UpdatedAt").First();
            Assert.Equal(Convert.ToString(Item.UpdatedAt, CultureInfo.InvariantCulture), updatedAtOperation.OldValue);
            Assert.Equal(Convert.ToString(_newItem.UpdatedAt, CultureInfo.InvariantCulture), updatedAtOperation.Value);
            var createdAtOperation = AddedOperations.Where(o => o.Property == "CreatedAt").First();
            Assert.Equal(Convert.ToString(Item.CreatedAt, CultureInfo.InvariantCulture), createdAtOperation.OldValue);
            Assert.Equal(Convert.ToString(_newItem.CreatedAt, CultureInfo.InvariantCulture), createdAtOperation.Value);
        }

        [Fact]
        public async Task Does_nothing_when_no_properties_changed()
        {
            await _changeTracker.TrackModifyAsync(Item, Item);

            Assert.Empty(AddedOperations);
            Assert.Empty(RemovedOperations);
            await DataStore.DidNotReceive().AddOperationsAsync(Arg.Any<NubeOperation[]>());
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
        public async Task Throws_when_operations_cannot_be_stored()
        {
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await _changeTracker.TrackModifyAsync(Item, _newItem));

            Assert.Equal($"Could not save modify operations for item {Item.Id}", ex.Message);
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