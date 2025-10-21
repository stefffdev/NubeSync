namespace Tests.NubeSync.Client.SQLiteStoreEFCore;

public partial class Always : NubeSQLiteDataStoreEFCoreTestBase
{
    [Fact]
    public async Task Add_table_creates_the_table()
    {
        await DataStore.AddTableAsync<TestItem>();

        Assert.True(await DataStore.TableExistsAsync<TestItem>());
    }
}