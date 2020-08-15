using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NubeSync.Core;
using NubeSync.Server;
using NubeSync.Server.Data;
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
            var types = new Dictionary<string, Type>
            {
                { "TestItem", typeof(TestItem) }
            };
            Service = new OperationService(types);
            Context.RemoveRange(Context.Items);
            Context.RemoveRange(Context.Operations);
            await Context.SaveChangesAsync();

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.Equal(2, Context.Items.Count());
            Assert.Equal(5, Context.Operations.Count());
            var item1 = Context.Items.Find("1");
            Assert.Equal("Name1", item1.Name);
            Assert.Equal("User", item1.UserId);
            Assert.True(item1.ServerUpdatedAt > DateTimeOffset.Now.AddMinutes(-1));
            var item2 = Context.Items.Find("2");
            Assert.Equal("Name2", item2.Name);
            Assert.Equal("User", item2.UserId);
            Assert.True(item2.ServerUpdatedAt > DateTimeOffset.Now.AddMinutes(-1));
        }

        [Fact]
        public async Task Gets_a_unregistered_type()
        {
            Context.RemoveRange(Context.Items);
            Context.RemoveRange(Context.Operations);
            await Context.SaveChangesAsync();

            await Service.ProcessOperationsAsync(Context, NewOperations, "User");

            Assert.Equal(2, Context.Items.Count());
            Assert.Equal(5, Context.Operations.Count());
            var item1 = Context.Items.Find("1");
            Assert.Equal("Name1", item1.Name);
            Assert.Equal("User", item1.UserId);
            Assert.True(item1.ServerUpdatedAt > DateTimeOffset.Now.AddMinutes(-1));
            var item2 = Context.Items.Find("2");
            Assert.Equal("Name2", item2.Name);
            Assert.Equal("User", item2.UserId);
            Assert.True(item2.ServerUpdatedAt > DateTimeOffset.Now.AddMinutes(-1));
        }

        [Fact]
        public async Task Modify_throws_for_empty_property()
        {
        }

        [Fact]
        public async Task Sets_processing_type_to_discarded_deleted_when_local_item_is_deleted()
        {
            var operations = new List<NubeOperation>
            { 
                new NubeOperation
                {
                    Id = "Op101",
                    ItemId = "1",
                    Type = OperationType.Modified,
                    TableName = "TestItem",
                    Property = "Name",
                    OldValue = null,
                    Value = "Name0",
                }
            };
            Context.RemoveRange(Context.Items);
            Context.RemoveRange(Context.Operations);
            Context.Add(new TestItem { Id = "1", DeletedAt = DateTimeOffset.Now });
            Context.SaveChanges();

            await Service.ProcessOperationsAsync(Context, operations);

            var operation = Context.Operations.Find("Op101");
            Assert.Equal(ProcessingType.DiscardedDeleted, operation.ProcessingType);
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
}