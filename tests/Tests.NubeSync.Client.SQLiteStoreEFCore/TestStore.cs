namespace Tests.NubeSync.Client.SQLiteStoreEFCore;

public class TestStore : NubeSQLiteDataStoreEFCore, IDisposable
{
    public DbSet<TestItem> Items { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase(Guid.NewGuid().ToString());
        options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}