using System;
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

        public bool HasCalledFind { get; set; }

        public bool HasCalledSave { get; set; }

        public DbSet<TestItem> Items { get; set; }

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
}