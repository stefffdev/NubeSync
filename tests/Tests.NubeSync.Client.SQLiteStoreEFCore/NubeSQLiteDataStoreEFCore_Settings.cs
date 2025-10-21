namespace Tests.NubeSync.Client.SQLiteStoreEFCore;

public partial class Always : NubeSQLiteDataStoreEFCoreTestBase
{
    [Fact]
    public async Task Stores_the_setting()
    {
        Assert.True(await DataStore.SetSettingAsync("test", "value"));

        var result = await DataStore.GetSettingAsync("test");

        Assert.Equal("value", result);
    }
}