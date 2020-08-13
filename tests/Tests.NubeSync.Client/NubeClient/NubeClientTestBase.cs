using System;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NubeSync.Client;
using NubeSync.Client.Data;

namespace Tests.NubeSync.Client.NubeClient_test
{
    public class NubeClientTestBase
    {
        protected IChangeTracker ChangeTracker;
        protected INubeClientConfiguration ClientConfiguration;
        protected IDataStore DataStore;
        protected INubeAuthentication Authentication;
        protected HttpClient HttpClient;
        protected MockHttpMessageHandler HttpMessageHandler;
        protected string InstallationId;
        protected NubeClient NubeClient;
        protected TestItem Item;

        public NubeClientTestBase()
        {
            InstallationId = "my/installation";
            Item = TestFactory.CreateTestItem("MyId", "MyName", DateTimeOffset.Now);

            Authentication = TestFactory.CreateAuthentication();
            DataStore = TestFactory.CreateDataStore();
            ClientConfiguration = TestFactory.CreateClientConfiguration();
            HttpMessageHandler = new MockHttpMessageHandler();
            HttpClient = new HttpClient(HttpMessageHandler);
            ChangeTracker = TestFactory.CreateChangeTracker();

            ClientConfiguration.Server.Returns("https://MyServer/");
            DataStore.AddOperationsAsync(Arg.Any<NubeOperation[]>()).Returns(true);

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