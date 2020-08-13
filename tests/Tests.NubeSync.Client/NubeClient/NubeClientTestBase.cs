using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NubeSync.Client;
using NubeSync.Client.Data;

namespace Tests.NubeSync.Client.NubeClient_test
{
    public class NubeClientTestBase
    {
        protected List<NubeOperation> AddedOperations;
        protected INubeAuthentication Authentication;
        protected IChangeTracker ChangeTracker;
        protected INubeClientConfiguration ClientConfiguration;
        protected IDataStore DataStore;
        protected HttpClient HttpClient;
        protected MockHttpMessageHandler HttpMessageHandler;
        protected string InstallationId;
        protected TestItem Item;
        protected NubeClient NubeClient;
        protected List<NubeOperation> RemovedOperations;

        public NubeClientTestBase()
        {
            AddedOperations = new List<NubeOperation>();
            RemovedOperations = new List<NubeOperation>();

            InstallationId = "my/installation";
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
            ClientConfiguration = TestFactory.CreateClientConfiguration();
            HttpMessageHandler = new MockHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler);
            ChangeTracker = TestFactory.CreateChangeTracker();
            ChangeTracker.TrackAddAsync(Arg.Any<TestItem>()).Returns(new List<NubeOperation>());
            ChangeTracker.TrackDeleteAsync(Arg.Any<TestItem>()).Returns(new NubeOperation());
            ChangeTracker.TrackModifyAsync(Arg.Any<TestItem>(), Arg.Any<TestItem>()).Returns(new List<NubeOperation>());

            ClientConfiguration.Server.Returns("https://MyServer/");

            NubeClient = new NubeClient(DataStore, ClientConfiguration, Authentication, HttpClient, ChangeTracker, InstallationId);
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