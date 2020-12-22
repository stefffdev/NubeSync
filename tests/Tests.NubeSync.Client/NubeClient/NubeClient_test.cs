using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NubeSync.Client;
using Xunit;

namespace Tests.NubeSync.Client.NubeClient_test
{
    public class Add_table : NubeClientTestBase
    {
        [Fact]
        public async Task Adds_the_table_to_the_datastore()
        {
            DataStore.TableExistsAsync<TestItem>().Returns(true);

            await NubeClient.AddTableAsync<TestItem>();

            await DataStore.Received().AddTableAsync<TestItem>();
        }

        [Fact]
        public async Task Throws_when_table_is_not_found_in_the_datastore()
        {
            DataStore.TableExistsAsync<TestItem>().Returns(false);

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await NubeClient.AddTableAsync<TestItem>());

            Assert.Equal("The table type TestItem cannot be found in the data store", ex.Message);
        }
    }

    public class Always : NubeClientTestBase
    {
        [Fact]
        public async Task Adds_the_installation_id_to_the_headers()
        {
            await AddTablesAsync();
            DataStore.InsertAsync(Arg.Any<TestItem>()).Returns(true);

            await NubeClient.PullTableAsync<TestItem>();

            var installationIdHeader = HttpClient.DefaultRequestHeaders.Where(h => h.Key == "NUBE-INSTALLATION-ID").First();
            Assert.NotNull(installationIdHeader.Value.First());

            await NubeClient.PushChangesAsync();
            var installationIdHeader2 = HttpClient.DefaultRequestHeaders.Where(h => h.Key == "NUBE-INSTALLATION-ID").First();

            Assert.Equal(installationIdHeader.Value.First(), installationIdHeader2.Value.First());
        }

        [Fact]
        public void Does_not_set_the_server_base_address_when_it_is_null()
        {
            var baseAddress = "https://something_else/";
            HttpClient.BaseAddress = new Uri(baseAddress);

            NubeClient = new NubeClient(DataStore, null, Authentication, HttpClient, ChangeTracker);

            Assert.Equal(baseAddress, HttpClient.BaseAddress.AbsoluteUri);
        }

        [Fact]
        public void Sets_the_server_base_address_when_it_is_not_null()
        {
            Assert.Equal(ServerUrl.ToLower(), HttpClient.BaseAddress.ToString());
        }

        [Fact]
        public void Throws_when_url_and_http_client_are_empty()
        {
            var ex = Assert.Throws<Exception>(() => new NubeClient(DataStore, null, Authentication, null, ChangeTracker));

            Assert.Equal("Either the server url or a HttpClient instance has to be provided.", ex.Message);
        }
    }
}