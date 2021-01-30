using NubeSync.Core;
using NubeSync.Server;
using NubeSync.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.NubeSync.Server.OperationService_test
{
    public class Last_changed_by_others : NubeSyncServerTestBase
    {
        [Fact]
        public void Returns_if_changed_by_others()
        {
            var result = Service.LastChangedByOthers(Context, "TestItem", "1", "NotMyInstallationId", UpdatedEarly);

            Assert.True(result);
        }

        [Fact]
        public void Returns_if_not_changed_by_others()
        {
            var result = Service.LastChangedByOthers(Context, "TestItem", "1", "InstallationId", UpdatedEarly);

            Assert.False(result);
        }

        [Fact]
        public void Returns_true_for_empty_installation_id()
        {
            var resultNull = Service.LastChangedByOthers(Context, "TestItem", "1", null, DateTimeOffset.Now);
            var resultEmpty = Service.LastChangedByOthers(Context, "TestItem", "1", null, DateTimeOffset.Now);

            Assert.True(resultNull);
            Assert.True(resultEmpty);
        }
    }

    public class Process_operations : NubeSyncServerTestBase
    {
        [Fact]
        public async Task Gets_a_registered_type()
        {
            Service = new OperationService(typeof(TestItem));
            await ClearDatabaseAsync();

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.Equal(2, Context.Items.Count());
            Assert.Equal(5, Context.Operations.Count());
            var item1 = Context.Items.Find("1");
            Assert.Equal("Name1", item1.Name);
            Assert.Equal("User", item1.UserId);
            Assert.True(item1.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
            var item2 = Context.Items.Find("2");
            Assert.Equal("Name2", item2.Name);
            Assert.Equal("User", item2.UserId);
            Assert.True(item2.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
        }

        [Fact]
        public async Task Gets_a_unregistered_type()
        {
            await ClearDatabaseAsync();

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.Equal(2, Context.Items.Count());
            Assert.Equal(5, Context.Operations.Count());
            var item1 = Context.Items.Find("1");
            Assert.Equal("Name1", item1.Name);
            Assert.Equal("User", item1.UserId);
            Assert.True(item1.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
            var item2 = Context.Items.Find("2");
            Assert.Equal("Name2", item2.Name);
            Assert.Equal("User", item2.UserId);
            Assert.True(item2.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
        }

        [Fact]
        public async Task Processes_multiple_updates()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations1 = GetModifyOperation();
            operations1[0].Id = "Op001";
            operations1[0].Value = "n1";
            var operations2 = GetModifyOperation();
            operations2[0].Value = "n2";
            operations1.AddRange(operations2);

            await Service.ProcessOperationsAsync(Context, operations1);

            var item = Context.Items.Find("1");
            Assert.Equal("n2", item.Name);
        }

        [Fact]
        public async Task Sets_server_updated_at()
        {
            await ClearDatabaseAsync();

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            foreach (var operation in Context.Operations)
            {
                Assert.True(operation.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
            }
        }

        [Fact]
        public async Task Sets_the_installation_id()
        {
            var installationId = "installationId";
            await ClearDatabaseAsync();
            Assert.Empty(Context.Operations);

            await Service.ProcessOperationsAsync(Context, NewOperations, installationId: installationId);

            Assert.Equal(installationId, Context.Operations.First().InstallationId);
        }

        [Fact]
        public async Task Sets_the_user_id()
        {
            var userId = "User";
            await ClearDatabaseAsync();
            Assert.Empty(Context.Operations);

            await Service.ProcessOperationsAsync(Context, NewOperations, userId);

            Assert.Equal(userId, Context.Operations.First().UserId);
        }

        [Fact]
        public async Task Skips_operations_that_already_were_processed()
        {
            Context.HasCalledSave = false;

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.False(Context.HasCalledSave);
        }

        [Fact]
        public async Task Stores_the_operations_in_the_database()
        {
            await ClearDatabaseAsync();
            Assert.Empty(Context.Operations);

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.Equal(NewOperations.Count, Context.Operations.Count());
        }

        [Fact]
        public async Task Throws_when_type_is_not_found()
        {
            var operation = new NubeOperation
            {
                Id = "1",
                TableName = "NonExistent"
            };

            var ex = await Assert.ThrowsAsync<NullReferenceException>(() => Service.ProcessOperationsAsync(Context, new List<NubeOperation> { operation }));

            Assert.Equal("The type NonExistent cannot be found", ex.Message);
        }
    }

    public class Process_operations_add : NubeSyncServerTestBase
    {
        [Fact]
        public async Task Add_adds_item_to_the_database()
        {
            var userId = "userId";
            await ClearDatabaseAsync();
            var operations = GetAddOperation();

            await Service.ProcessOperationsAsync(Context, operations, userId);

            var item = Context.Items.First();
            Assert.Equal(operations.First().ItemId, item.Id);
            Assert.Equal(userId, item.UserId);
            Assert.True(item.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
        }
    }

    public class Process_operations_delete : NubeSyncServerTestBase
    {
        [Fact]
        public async Task Delete_deletes_the_item()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var userId = "userId";
            var operations = GetDeleteOperation();

            await Service.ProcessOperationsAsync(Context, operations, userId);

            var item = Context.Items.Find(operations.First().ItemId);
            Assert.Equal(item.DeletedAt, item.ServerUpdatedAt);
            Assert.True(item.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
            Assert.True(item.DeletedAt > DateTimeOffset.Now.AddSeconds(-1));
        }
    }

    public class Process_operations_modify : NubeSyncServerTestBase
    {
        [Fact]
        public async Task Modify_does_nothing_when_operations_are_empty()
        {
            await ClearDatabaseAsync();
            Context.HasCalledFind = false;

            await Service.ProcessOperationsAsync(Context, GetAddOperation());

            Assert.False(Context.HasCalledFind);
        }

        [Fact]
        public async Task Modify_sets_processing_type_to_discarded_deleted_when_local_item_is_deleted()
        {
            var operations = GetModifyOperation();
            Context.RemoveRange(Context.Items);
            Context.RemoveRange(Context.Operations);
            Context.Add(new TestItem { Id = "1", DeletedAt = DateTimeOffset.Now });
            Context.SaveChanges();

            await Service.ProcessOperationsAsync(Context, operations);

            var operation = Context.Operations.Find("Op101");
            Assert.Equal(ProcessingType.DiscardedDeleted, operation.ProcessingType);
        }

        [Fact]
        public async Task Modify_sets_processing_type_to_discarded_outdated_when_newer_operation_was_processed()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].CreatedAt = DateTimeOffset.Now;
            await Service.ProcessOperationsAsync(Context, operations);
            var secondOperations = GetModifyOperation();
            secondOperations[0].Id = "Op102";
            secondOperations[0].CreatedAt = DateTimeOffset.Now.AddMinutes(-5);

            await Service.ProcessOperationsAsync(Context, secondOperations);

            var operation = Context.Operations.Find("Op102");
            Assert.Equal(ProcessingType.DiscaredOutdated, operation.ProcessingType);
        }

        [Fact]
        public async Task Modify_sets_server_updated_at()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();

            await Service.ProcessOperationsAsync(Context, GetModifyOperation());

            foreach (var operation in Context.Operations)
            {
                Assert.True(operation.ServerUpdatedAt > DateTimeOffset.Now.AddSeconds(-1));
            }
        }

        [Fact]
        public async Task Modify_sets_the_property()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var newValue = "a new value";
            var operations = GetModifyOperation();
            operations[0].Value = newValue;

            await Service.ProcessOperationsAsync(Context, operations);

            var item = Context.Items.Find(operations[0].ItemId);
            Assert.Equal(newValue, item.Name);
        }

        [Fact]
        public async Task Modify_throws_for_empty_property()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].Property = null;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ProcessOperationsAsync(Context, operations));

            Assert.Equal("Property of operation Op101 cannot be empty", ex.Message);
        }

        [Fact]
        public async Task Modify_throws_then_the_property_name_is_empty()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].Property = null;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ProcessOperationsAsync(Context, operations));

            Assert.Equal("Property of operation Op101 cannot be empty", ex.Message);
        }

        [Fact]
        public async Task Modify_throws_when_item_cannot_be_found()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].ItemId = null;

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ProcessOperationsAsync(Context, operations));

            Assert.Equal($"Item with the id {operations[0].ItemId} cannot be found", ex.Message);
        }

        [Fact]
        public async Task Modify_throws_when_property_does_not_exist()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].Property = "UnknownProperty";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ProcessOperationsAsync(Context, operations));

            Assert.Equal($"Property {operations[0].Property} not found on item Tests.NubeSync.Server.TestItem", ex.Message);
        }

        [Fact]
        public async Task Modify_throws_when_value_cannot_be_converted()
        {
            Context.RemoveRange(Context.Operations);
            Context.SaveChanges();
            var operations = GetModifyOperation();
            operations[0].Property = "Value";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ProcessOperationsAsync(Context, operations));

            Assert.Equal($"Unable to convert value Value of operation {operations[0].Id}: Name0 is not a valid value for Int32. (Parameter 'value')", ex.Message);
        }
    }
}