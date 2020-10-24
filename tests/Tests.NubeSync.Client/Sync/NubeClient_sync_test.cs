using NSubstitute;
using NubeSync.Client;
using NubeSync.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tests.NubeSync.Client.NubeClient_test;
using Xunit;

namespace Tests.NubeSync.Client.NubeClient_sync_test
{
    public class Has_pending_operations : NubeClientTestBase
    {
        [Fact]
        public async Task Returns_false_when_no_operations_are_pending()
        {
            var result = await NubeClient.HasPendingOperationsAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task Returns_true_when_operations_are_pending()
        {
            var operations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
            };
            DataStore.GetOperationsAsync().Returns(operations.AsQueryable());

            var result = await NubeClient.HasPendingOperationsAsync();

            Assert.True(result);
        }
    }

    public class Pull_table : NubeClientTestBase
    {
        public Pull_table()
        {
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);
            DataStore.InsertAsync(Arg.Any<TestItem2>()).Returns(true);
            DataStore.UpdateAsync(Arg.Any<TestItem>()).Returns(true);
        }

        [Fact]
        public async Task Adds_authentication_to_the_request_header()
        {
            var token = "myToken";
            await AddTablesAsync();
            Authentication.GetBearerTokenAsync().Returns(token);

            await NubeClient.PullTableAsync<TestItem>();

            await Authentication.Received().GetBearerTokenAsync();
            Assert.Equal("Bearer", HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal(token, HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public async Task Adds_the_pagination_parameters()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem>();

            Assert.Equal("https://myserver/TestItem?pageNumber=1&pageSize=100", HttpMessageHandler.LastRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Checks_if_table_is_valid()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await NubeClient.PullTableAsync<TestItem>());

            Assert.Equal("Table TestItem is not registered in the nube client", ex.Message);
        }

        [Fact]
        public async Task Creates_a_new_item()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.Received().InsertAsync(Arg.Is<TestItem>(i => i.Id == HttpMessageHandler.Results[0].Id));
            await DataStore.Received().InsertAsync(Arg.Is<TestItem>(i => i.Id == HttpMessageHandler.Results[1].Id));
        }

        [Fact]
        public async Task Deletes_the_item_if_it_is_deleted_on_the_server()
        {
            await AddTablesAsync();
            var item = new TestItem { Id = "123", DeletedAt = DateTimeOffset.Now };
            HttpMessageHandler.Results = new List<TestItem> { item };
            DataStore.FindByIdAsync<TestItem>(item.Id).Returns(item);
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.Received().DeleteAsync(item);
        }

        [Fact]
        public async Task Does_not_delete_the_item_if_it_is_not_deleted_on_the_server()
        {
            await AddTablesAsync();
            var item = new TestItem { Id = "123" };
            HttpMessageHandler.Results = new List<TestItem> { item };
            DataStore.FindByIdAsync<TestItem>(item.Id).Returns(item);
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.DidNotReceive().DeleteAsync(item);
        }

        [Fact]
        public async Task Does_not_try_to_delete_the_item_when_it_is_not_found_in_the_local_store()
        {
            await AddTablesAsync();
            var item = new TestItem { Id = "123" };
            HttpMessageHandler.Results = new List<TestItem> { item };
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.DidNotReceive().DeleteAsync(item);
        }

        [Fact]
        public async Task Includes_the_last_sync_timestamp_in_the_request()
        {
            await AddTablesAsync();
            DataStore.GetSettingAsync("lastSync-TestItem").Returns("2001-1-1");

            await NubeClient.PullTableAsync<TestItem>();

            Assert.Equal("https://myserver/TestItem?pageNumber=1&pageSize=100&laterThan=2000-12-31T23:00:00.000Z", HttpMessageHandler.LastRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Queries_the_last_sync_timestamp()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.Received().GetSettingAsync("lastSync-TestItem");
        }

        [Fact]
        public async Task Returns_0_when_cancelled()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var result = await NubeClient.PullTableAsync<TestItem>(tokenSource.Token);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Returns_the_number_of_pulled_records()
        {
            HttpMessageHandler.UserLargeResultSet();
            await AddTablesAsync();

            var result = await NubeClient.PullTableAsync<TestItem>();

            Assert.Equal(HttpMessageHandler.Results.Count, result);
        }

        [Fact]
        public async Task Saves_the_last_sync_timestamp()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.Received().SetSettingAsync("lastSync-TestItem", Arg.Any<string>());
        }

        [Fact]
        public async Task Stops_when_no_more_items_are_returned()
        {
            HttpMessageHandler.UserLargeResultSet(170);
            await AddTablesAsync();

            var result = await NubeClient.PullTableAsync<TestItem>();

            Assert.Equal(170, result);
        }

        [Fact]
        public async Task Stops_when_the_number_of_returned_items_is_not_equal_to_the_page_size()
        {
            HttpMessageHandler.UserLargeResultSet(7);
            await AddTablesAsync();

            var result = await NubeClient.PullTableAsync<TestItem>();

            Assert.Equal(7, result);
        }

        [Fact]
        public async Task Throws_when_the_http_request_fails()
        {
            await AddTablesAsync();
            HttpMessageHandler.HttpRequestFails = true;

            var ex = await Assert.ThrowsAsync<PullOperationFailedException>(async () => await NubeClient.PullTableAsync<TestItem>());

            Assert.Equal("BadRequest", ex.Message);
        }

        [Fact]
        public async Task Updates_all_properties_of_the_local_item()
        {
            await AddTablesAsync();
            var localItems = new List<TestItem>
            {
                new TestItem { Id = "123", Name = "LocalName1" },
                new TestItem { Id = "456", Name = "LocalName2" },
            };
            DataStore.FindByIdAsync<TestItem>(localItems[0].Id).Returns(localItems[0]);
            DataStore.FindByIdAsync<TestItem>(localItems[1].Id).Returns(localItems[1]);

            await NubeClient.PullTableAsync<TestItem>();

            await DataStore.DidNotReceive().InsertAsync(Arg.Any<TestItem>());
            await DataStore.Received().UpdateAsync(Arg.Is<TestItem>(i => i.Id == localItems[0].Id && i.Name == HttpMessageHandler.Results[0].Name));
            await DataStore.Received().UpdateAsync(Arg.Is<TestItem>(i => i.Id == localItems[1].Id && i.Name == HttpMessageHandler.Results[1].Name));
            await ChangeTracker.DidNotReceive().TrackAddAsync(Arg.Any<TestItem>());
            await ChangeTracker.DidNotReceive().TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>());
        }

        [Fact]
        public async Task Uses_default_path_for_table()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem>();

            Assert.StartsWith("https://myserver/TestItem", HttpMessageHandler.LastRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Uses_special_table_path_when_set()
        {
            await AddTablesAsync();

            await NubeClient.PullTableAsync<TestItem2>();

            Assert.StartsWith("https://myserver/differentPath", HttpMessageHandler.LastRequest.RequestUri.AbsoluteUri);
        }
    }

    public class Push_changes : NubeClientTestBase
    {
        [Fact]
        public async Task Adds_authentication_to_the_request_header()
        {
            var token = "myToken";
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);
            Authentication.GetBearerTokenAsync().Returns(token);

            await NubeClient.PullTableAsync<TestItem>();

            await Authentication.Received().GetBearerTokenAsync();
            Assert.Equal("Bearer", HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal(token, HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public async Task Deletes_the_operations_when_post_was_successful()
        {
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
            };
            DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
            DataStore.When(x => x.DeleteOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                x =>
                {
                    existingOperations = new List<NubeOperation>();
                    DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
                });

            await NubeClient.PushChangesAsync();

            await DataStore.Received().DeleteOperationsAsync(Arg.Any<NubeOperation[]>());
        }

        [Fact]
        public async Task Posts_the_operations()
        {
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Deleted },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Modified },
                new NubeOperation() { ItemId = Item.Id, Type = OperationType.Added },
            };
            var expectedContent = JsonSerializer.Serialize(existingOperations,
                new JsonSerializerOptions { IgnoreNullValues = true });
            DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
            DataStore.When(x => x.DeleteOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                x =>
                {
                    existingOperations = new List<NubeOperation>();
                    DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
                });

            await NubeClient.PushChangesAsync();

            var content = await HttpMessageHandler.LastRequest.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, content);
            Assert.Equal("https://myserver/operations", HttpMessageHandler.LastRequest.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Queries_the_operations_until_there_are_no_more()
        {
            var i = 0;
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
            };
            DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
            DataStore.When(x => x.DeleteOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                x =>
                {
                    if (i > 0)
                    {
                        existingOperations = new List<NubeOperation>();
                        DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());
                    }

                    i++;
                });

            await NubeClient.PushChangesAsync();

            await DataStore.Received(2).DeleteOperationsAsync(Arg.Any<NubeOperation[]>());
        }

        [Fact]
        public async Task Reads_the_operations_from_the_store()
        {
            await NubeClient.PushChangesAsync();

            await DataStore.Received().GetOperationsAsync(100);
        }

        [Fact]
        public async Task Returns_false_when_cancelled()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var result = await NubeClient.PushChangesAsync(tokenSource.Token);

            Assert.False(result);
        }

        [Fact]
        public async Task Returns_true_when_there_are_no_operations()
        {
            DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(new List<NubeOperation>().AsQueryable());

            var result = await NubeClient.PushChangesAsync();

            Assert.True(result);
        }

        [Fact]
        public async Task Throws_when_post_failed()
        {
            HttpMessageHandler.HttpRequestFails = true;
            var existingOperations = new List<NubeOperation>()
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
            };
            DataStore.GetOperationsAsync(Arg.Any<int>()).Returns(existingOperations.AsQueryable());

            var ex = await Assert.ThrowsAsync<PushOperationFailedException>(async () => await NubeClient.PushChangesAsync());

            Assert.Equal("Cannot push operations to the server: BadRequest some message", ex.Message);
        }
    }
}