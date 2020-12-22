using NSubstitute;
using NubeSync.Client;
using NubeSync.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tests.NubeSync.Client.NubeClient_test;
using Xunit;

namespace Tests.NubeSync.Client.NubeClient_Data_test
{
    public class Delete : NubeClientTestBase
    {
        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.DeleteAsync(Item));

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Cleans_up_obsolete_operations()
        {
            await AddTablesAsync();
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", TableName = "TestItem", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Type = OperationType.Deleted },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Type = OperationType.Added },
            };
            var expectedOperations = existingOperations.Skip(2).ToList();
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());

            await NubeClient.DeleteAsync(Item);

            Assert.Equal(expectedOperations, RemovedOperations);
        }

        [Fact]
        public async Task Deletes_the_item_from_the_store()
        {
            await AddTablesAsync();
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.DeleteAsync(Item);

            await DataStore.Received().DeleteAsync(Item);
        }

        [Fact]
        public async Task Does_not_clean_up_delete_operations_from_different_tables()
        {
            await AddTablesAsync();
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", TableName = "OtherTable", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "OtherTable", Type = OperationType.Deleted },
                new NubeOperation() { ItemId = Item.Id, TableName = "OtherTable", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "OtherTable", Type = OperationType.Added },
            };
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());

            await NubeClient.DeleteAsync(Item);

            Assert.Empty(RemovedOperations);
        }

        [Fact]
        public async Task Does_not_track_the_delete_when_change_tracker_is_disabled()
        {
            await AddTablesAsync();
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.DeleteAsync(Item, true);

            await ChangeTracker.DidNotReceive().TrackDeleteAsync(Arg.Any<TestItem>());
        }

        [Fact]
        public async Task Throws_when_item_id_is_empty()
        {
            await AddTablesAsync();
            Item.Id = null;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.DeleteAsync(Item));

            Assert.Equal("Cannot delete item without id", ex.Message);
        }

        [Fact]
        public async Task Throws_when_item_id_is_null()
        {
            await AddTablesAsync();
            Item.Id = null;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.DeleteAsync(Item));

            Assert.Equal("Cannot delete item without id", ex.Message);
        }

        [Fact]
        public async Task Throws_when_operations_cannot_be_stored()
        {
            await AddTablesAsync();
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.DeleteAsync(Item));

            Assert.Equal($"Could not save delete operation for item {Item.Id}", ex.Message);
        }

        [Fact]
        public async Task Throws_when_store_fails()
        {
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(false);
            await AddTablesAsync();

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.DeleteAsync(Item));

            Assert.Equal("Could not delete item", ex.Message);
        }

        [Fact]
        public async Task Tracks_the_delete()
        {
            await AddTablesAsync();
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.DeleteAsync(Item);

            await ChangeTracker.Received().TrackDeleteAsync(Item);
        }
    }

    public class Find_by : NubeClientTestBase
    {
        private readonly Expression<Func<TestItem, bool>> _predicate = f => true;

        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.FindByAsync(_predicate));

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Forwards_the_predicate_to_the_datastore()
        {
            await AddTablesAsync();

            await NubeClient.FindByAsync(_predicate);

            await DataStore.Received().FindByAsync(_predicate);
        }
    }

    public class Get_all : NubeClientTestBase
    {
        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.GetAllAsync<TestItem>());

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Queries_all_items_from_the_datastore()
        {
            await AddTablesAsync();

            await NubeClient.GetAllAsync<TestItem>();

            await DataStore.Received().AllAsync<TestItem>();
        }
    }

    public class Get_by_id : NubeClientTestBase
    {
        private readonly string _id = "id";

        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.GetByIdAsync<TestItem>(_id));

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Queries_all_items_from_the_datastore()
        {
            await AddTablesAsync();

            await NubeClient.GetByIdAsync<TestItem>(_id);

            await DataStore.Received().FindByIdAsync<TestItem>(_id);
        }
    }

    public class Save : NubeClientTestBase
    {
        private readonly TestItem _newItem;

        public Save()
        {
            _newItem = TestFactory.CreateTestItem(Item.Id, "NewName", DateTimeOffset.Now.AddSeconds(1));
        }

        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.SaveAsync(Item));

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Cleans_up_obsolete_operations()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            _newItem.CreatedAt = Item.CreatedAt;
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", TableName = "TestItem", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Property = "CreatedAt", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Property = "Name", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "TestItem", Property = "UpdatedAt", Type = OperationType.Modified },
            };
            var expectedOperations = existingOperations.Skip(2).ToList();
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());
            var changeTracker = new ChangeTracker();
            var addOperations = await changeTracker.TrackModifyAsync(Item, _newItem);
            ChangeTracker.TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>()).Returns(addOperations);
            existingOperations.AddRange(addOperations);

            await NubeClient.SaveAsync(_newItem);

            Assert.Equal(expectedOperations, RemovedOperations);
        }

        [Fact]
        public async Task Creates_a_id_when_the_item_is_new()
        {
            var oldItemId = Item.Id;
            Item.Id = null;
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            Assert.NotNull(Item.Id);
            Assert.NotEqual(oldItemId, Item.Id);
        }

        [Fact]
        public async Task Does_not_clean_up_operations_from_different_tables_or_currently_added_operations()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            _newItem.CreatedAt = Item.CreatedAt;
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = Item.Id, TableName = "DifferentTable", Property = "CreatedAt", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, TableName = "DifferentTable", Property = "Name", Type = OperationType.Modified },
            };
            DataStore.GetOperationsAsync().Returns(existingOperations.AsQueryable());
            var changeTracker = new ChangeTracker();
            var addOperations = await changeTracker.TrackModifyAsync(Item, _newItem);
            ChangeTracker.TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>()).Returns(addOperations);
            existingOperations.AddRange(addOperations);

            await NubeClient.SaveAsync(_newItem);

            Assert.Empty(RemovedOperations);
        }

        [Fact]
        public async Task Does_not_set_created_at_when_change_tracker_is_disabled()
        {
            await AddTablesAsync();
            var createdAt = Item.CreatedAt;
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item, disableChangeTracker: true);

            Assert.Equal(createdAt, Item.CreatedAt);
        }

        [Fact]
        public async Task Does_not_set_updated_at_when_change_tracker_is_disabled()
        {
            await AddTablesAsync();
            var updatedAt = Item.UpdatedAt;
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item, disableChangeTracker: true);

            Assert.Equal(updatedAt, Item.UpdatedAt);
        }

        [Fact]
        public async Task Does_not_track_insert_when_change_tracker_is_disabled()
        {
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item, disableChangeTracker: true);

            await ChangeTracker.DidNotReceive().TrackDeleteAsync(Arg.Any<TestItem>());
        }

        [Fact]
        public async Task Does_not_track_update_when_change_tracker_is_disabled()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item, disableChangeTracker: true);

            await ChangeTracker.DidNotReceive().TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>());
        }

        [Fact]
        public async Task Does_not_try_to_load_the_existing_item_from_the_database_when_it_is_not_null()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item, existingItem: Item);

            await DataStore.DidNotReceive().FindByIdAsync<TestItem>(Item.Id);
        }

        [Fact]
        public async Task Does_not_try_to_load_the_existing_item_from_the_database_when_the_id_is_null()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);
            Item.Id = null;

            await NubeClient.SaveAsync(Item);

            await DataStore.DidNotReceive().FindByIdAsync<TestItem>(Arg.Any<string>());
        }

        [Fact]
        public async Task Inserts_the_item_in_the_store()
        {
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            await DataStore.Received().InsertAsync(Item);
        }

        [Fact]
        public async Task Tries_to_load_the_existing_item_from_the_database_when_it_is_null()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            await DataStore.Received().FindByIdAsync<TestItem>(Item.Id);
        }

        [Fact]
        public async Task Sets_created_at_when_change_tracker_is_enabled()
        {
            await AddTablesAsync();
            var createdAt = Item.CreatedAt;
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            Assert.NotEqual(createdAt, Item.CreatedAt);
        }

        [Fact]
        public async Task Sets_updated_at_when_change_tracker_is_enabled()
        {
            await AddTablesAsync();
            var updatedAt = Item.UpdatedAt;
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            Assert.NotEqual(updatedAt, Item.UpdatedAt);
        }

        [Fact]
        public async Task Throws_when_operations_cannot_be_removed()
        {
            _AddItemToStore();
            await AddTablesAsync();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);
            DataStore.DeleteOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.SaveAsync(_newItem));

            Assert.Equal($"Could not delete obsolete operations for modified item", ex.Message);
        }

        [Fact]
        public async Task Throws_when_operations_cannot_be_stored()
        {
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(false);

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.SaveAsync(_newItem));

            Assert.Equal($"Could not save add operations for item {Item.Id}", ex.Message);
        }

        [Fact]
        public async Task Throws_when_store_insert_fails()
        {
            await AddTablesAsync();

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.SaveAsync(Item));

            Assert.Equal("Could not insert item", ex.Message);
        }

        [Fact]
        public async Task Throws_when_store_update_fails()
        {
            await AddTablesAsync();
            _AddItemToStore();

            var ex = await Assert.ThrowsAsync<StoreOperationFailedException>(async () => await NubeClient.SaveAsync(Item));

            Assert.Equal("Could not update item", ex.Message);
        }

        [Fact]
        public async Task Tracks_the_insert_operation()
        {
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            await ChangeTracker.Received().TrackAddAsync(Item);
        }

        [Fact]
        public async Task Tracks_the_update_operation()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            await ChangeTracker.Received().TrackModifyAsync(Arg.Is<TestItem>(
                t => t.Id == Item.Id && t.Name == Item.Name && t.UpdatedAt == Item.UpdatedAt && t.CreatedAt == Item.CreatedAt), Item);
        }

        [Fact]
        public async Task Updates_the_item_in_the_store()
        {
            await AddTablesAsync();
            _AddItemToStore();
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.SaveAsync(Item);

            await DataStore.Received().UpdateAsync(Item);
        }

        private void _AddItemToStore()
        {
            DataStore.FindByIdAsync<TestItem>(Item.Id).Returns(Item);
        }
    }
}