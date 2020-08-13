using System.Threading.Tasks;
using Xunit;

namespace Tests.NubeSync.Client.SQLiteStoreEFCore.NubeSQLiteDataStoreEFCore_Settings
{
    public class Always : NubeSQLiteDataStoreEFCoreTestBase
    {
        [Fact]
        public async Task Stores_the_setting()
        {
            await DataStore.InitializeAsync();
            Assert.True(await DataStore.SetSettingAsync("test", "value"));

            var result = await DataStore.GetSettingAsync("test");

            Assert.Equal("value", result);
        }
    }
}