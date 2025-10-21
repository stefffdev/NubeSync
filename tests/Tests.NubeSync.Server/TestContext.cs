using System.Threading;

namespace Tests.NubeSync.Server;

public class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    public bool HasCalledFind { get; set; }

    public bool HasCalledSave { get; set; }

    public DbSet<TestItem> Items { get; set; }

    public DbSet<TestItemInvalid> InvalidItems { get; set; }

    public DbSet<NubeServerOperation> Operations { get; set; }

    public override ValueTask<object> FindAsync(Type entityType, params object[] keyValues)
    {
        HasCalledFind = true;
        return base.FindAsync(entityType, keyValues);
    }

    public override int SaveChanges()
    {
        HasCalledSave = true;
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HasCalledSave = true;
        return base.SaveChangesAsync(cancellationToken);
    }
}