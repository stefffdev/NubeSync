using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NubeSync.Client;
using NubeSync.Core;

namespace Tests.NubeSync.Client.NubeClient_test
{
    public class NubeClientTestBase
    {
        protected List<NubeOperation> AddedOperations;
        protected INubeAuthentication Authentication;
        protected IChangeTracker ChangeTracker;
        protected IDataStore DataStore;
        protected HttpClient HttpClient;
        protected MockHttpMessageHandler HttpMessageHandler;
        protected TestItem Item;
        protected NubeClient NubeClient;
        protected List<NubeOperation> RemovedOperations;
        protected string ServerUrl = "https://MyServer/";

        public NubeClientTestBase()
        {
            AddedOperations = new List<NubeOperation>();
            RemovedOperations = new List<NubeOperation>();

            Item = TestFactory.CreateTestItem("MyId", "MyName", DateTimeOffset.Now);
            Authentication = TestFactory.CreateAuthentication();
            DataStore = TestFactory.CreateDataStore();
            DataStore.When(x => x.AddOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                y => AddedOperations.AddRange(y.Arg<NubeOperation[]>()));
            DataStore.When(x => x.DeleteOperationsAsync(Arg.Any<NubeOperation[]>())).Do(
                y => RemovedOperations.AddRange(y.Arg<NubeOperation[]>()));
            DataStore.DeleteOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(true);
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(true);
            DataStore.DeleteAsync(Arg.Any<TestItem>()).Returns(true);
            HttpMessageHandler = new MockHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler);
            ChangeTracker = TestFactory.CreateChangeTracker();
            ChangeTracker.TrackAddAsync(Arg.Any<TestItem>()).Returns(new List<NubeOperation>());
            ChangeTracker.TrackDeleteAsync(Arg.Any<TestItem>()).Returns(new List<NubeOperation>());
            ChangeTracker.TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>()).Returns(new List<NubeOperation>());

            NubeClient = new NubeClient(DataStore, ServerUrl, Authentication, HttpClient, ChangeTracker);
        }

        protected async Task AddTablesAsync()
        {
            DataStore.TableExistsAsync<TestItem>().Returns(true);
            DataStore.TableExistsAsync<TestItem2>().Returns(true);

            await NubeClient.AddTableAsync<TestItem>();
            await NubeClient.AddTableAsync<TestItem2>("differentPath");
        }
    }
}