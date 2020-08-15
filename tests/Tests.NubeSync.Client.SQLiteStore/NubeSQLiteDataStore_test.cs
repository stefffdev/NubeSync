using System.IO;
using System.Threading.Tasks;
using NubeSync.Client.SQLiteStore;
using NubeSync.Core;
using Xunit;

namespace Tests.NubeSync.Client.SQLiteStore.NubeSQLiteDataStore_test
{
    public class Always : NubeSQLiteDataStoreTestBase
    {
        [Fact]
        public async Task Add_table_creates_the_table()
        {
            await DataStore.AddTableAsync<TestItem>();

            Assert.True(await DataStore.TableExistsAsync<TestItem>());
        }

        [Fact]
        public void Db_path_can_be_set()
        {
            var filename = "test.db";

            var dataStore = new NubeSQLiteDataStore(filename);

            Assert.Equal(filename, Path.GetFileName(dataStore.Database.DatabasePath));
        }

        [Fact]
        public async Task Initialize_creates_internal_tables()
        {
            await DataStore.InitializeAsync();

            Assert.Equal(0, await DataStore.Database.Table<NubeOperation>().CountAsync());
            Assert.Equal(0, await DataStore.Database.Table<NubeSetting>().CountAsync());
        }

        [Fact]
        public void Uses_a_default_path()
        {
            var dataStore = new NubeSQLiteDataStore();

            Assert.Equal("nube-offline.db", Path.GetFileName(dataStore.Database.DatabasePath));
        }
    }
}