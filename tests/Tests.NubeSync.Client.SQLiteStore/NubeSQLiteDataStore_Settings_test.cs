namespace Tests.NubeSync.Client.SQLiteStore;

public partial class Always : NubeSQLiteDataStoreTestBase
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