using Microsoft.EntityFrameworkCore;
using NubeSync.Client.SQLiteStoreEFCore;

namespace Tests.NubeSync.Client.SQLiteStoreEFCore
{
    public class TestStore : NubeSQLiteDataStoreEFCore
    {
        public TestStore(string database) : base(database)
        {
        }

        public DbSet<TestItem> Items { get; set; } = null!;
    }
}