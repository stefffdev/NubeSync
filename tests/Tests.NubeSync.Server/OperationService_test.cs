using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NubeSync.Server;
using NubeSync.Server.Data;
using Xunit;

namespace Tests.NubeSync.Server.OperationService_test
{
    public class Last_changed_by_others : NubeSyncServerTestBase
    {
        [Fact]
        public void Returns_true_for_empty_installation_id()
        {
            var resultNull = Service.LastChangedByOthers(Context, "TestItem", "1", null, DateTimeOffset.Now);
            var resultEmpty = Service.LastChangedByOthers(Context, "TestItem", "1", null, DateTimeOffset.Now);

            Assert.True(resultNull);
            Assert.True(resultEmpty);
        }

        [Fact]
        public void Returns_if_not_changed_by_others()
        {
            var result = Service.LastChangedByOthers(Context, "TestItem", "1", "InstallationId", UpdatedEarly);

            Assert.False(result);
        }

        [Fact]
        public void Returns_if_changed_by_others()
        {
            var result = Service.LastChangedByOthers(Context, "TestItem", "1", "NotMyInstallationId", UpdatedEarly);

            Assert.True(result);
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

            await Service.ProcessOperationsAsync(Context, NewOperations);

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
            var operations = Context.Operations.Select(o => o.ToString());
            var operationResult = string.Join(string.Empty, operations);
            Assert.Equal("Id Op100, Added in table TestItem for item 1 with value  (old: ) 01.01.0001 00:00:00 +00:00Id Op101, Modified in table TestItem for item 1 with value Name0 (old: ) 01.01.0001 00:00:00 +00:00Id Op200, Modified in table TestItem for item 1 with value Name1 (old: Name0) 01.01.0001 00:00:00 +00:00Id Op300, Added in table TestItem for item 2 with value  (old: ) 01.01.0001 00:00:00 +00:00Id Op400, Modified in table TestItem for item 2 with value Name2 (old: ) 01.01.0001 00:00:00 +00:00", operationResult);
        }

        [Fact]
        public async Task Gets_a_unregistered_type()
        {
            Context.RemoveRange(Context.Items);
            Context.RemoveRange(Context.Operations);

            await Service.ProcessOperationsAsync(Context, NewOperations);

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
            var operations = Context.Operations.Select(o => o.ToString());
            var operationResult = string.Join(string.Empty, operations);
            Assert.Equal("Id Op100, Added in table TestItem for item 1 with value  (old: ) 01.01.0001 00:00:00 +00:00Id Op101, Modified in table TestItem for item 1 with value Name0 (old: ) 01.01.0001 00:00:00 +00:00Id Op200, Modified in table TestItem for item 1 with value Name1 (old: Name0) 01.01.0001 00:00:00 +00:00Id Op300, Added in table TestItem for item 2 with value  (old: ) 01.01.0001 00:00:00 +00:00Id Op400, Modified in table TestItem for item 2 with value Name2 (old: ) 01.01.0001 00:00:00 +00:00", operationResult);
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

        [Fact]
        public async Task Sets_processing_type_to_discarded_deleted_when_local_item_is_deleted()
        {
            Context.RemoveRange(Context.Items);
            Context.Add(new TestItem { Id = "1", DeletedAt = DateTimeOffset.Now });
            Context.SaveChanges();

            await Service.ProcessOperationsAsync(Context, NewOperations);

            var operation = Context.Operations.Find("Op100");
            Assert.Equal(ProcessingType.DiscardedDeleted, operation.ProcessingType);
        }

        [Fact]
        public async Task Modify_throws_for_empty_property()
        {

        }
    }
}
