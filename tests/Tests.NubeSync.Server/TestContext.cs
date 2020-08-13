using Microsoft.EntityFrameworkCore;
using NubeSync.Server.Data;

namespace Tests.NubeSync.Server
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options)
        { }

        public DbSet<TestItem> Items { get; set; }

        public DbSet<NubeOperation> Operations { get; set; }
    }
}