using NubeSync.Core;

namespace Tests.NubeSync.Client.SQLiteStoreEFCore
{
    public class NubeSQLiteDataStoreEFCoreTestBase
    {
        protected TestStore DataStore;
        protected NubeOperation[] Operations;

        public NubeSQLiteDataStoreEFCoreTestBase()
        {
            DataStore = new TestStore();
            DataStore.Database.EnsureCreated();

            Operations = new NubeOperation[]
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "123", Type = OperationType.Deleted },
                new NubeOperation() { ItemId = "456", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "789", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Added },
            };
        }
    }
}