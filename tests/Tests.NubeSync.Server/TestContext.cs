using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Server.Data;

namespace Tests.NubeSync.Server
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options)
        { }

        public bool HasSaved { get; set; }

        public DbSet<TestItem> Items { get; set; }

        public DbSet<NubeServerOperation> Operations { get; set; }

        public override int SaveChanges()
        {
            HasSaved = true;
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HasSaved = true;
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}