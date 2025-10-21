namespace NubeSync.Client.SQLiteStoreEFCore;

public partial class NubeSQLiteDataStoreEFCore : DbContext, IDataStore
{
    private readonly string _databasePath;

    public NubeSQLiteDataStoreEFCore(string databasePath = "nube-offline.db")
    {
        _databasePath = databasePath;

        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<NubeOperation> NubeOperations { get; set; } = null!;

    internal DbSet<NubeSetting> NubeSettings { get; set; } = null!;

    public Task AddTableAsync<T>(string? tableUrl = null) where T : NubeTable
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        if ((await Database.GetPendingMigrationsAsync()).Any())
        {
            var migrator = Database.GetInfrastructure().GetService<IMigrator>();
            if (migrator != null)
            {
                await migrator.MigrateAsync().ConfigureAwait(false);
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_databasePath}");
    }
}