using System;
using System.IO;
using NubeSync.Client.Data;

namespace Tests.NubeSync.Client.SQLiteStoreEFCore
{
    public class NubeSQLiteDataStoreEFCoreTestBase : IDisposable
    {
        protected string DatabaseFile;
        protected TestStore DataStore;
        protected NubeOperation[] Operations;

        public NubeSQLiteDataStoreEFCoreTestBase()
        {
            DatabaseFile = Path.GetTempFileName();
            DataStore = new TestStore(DatabaseFile);
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

        public void Dispose()
        {
            DataStore.Dispose();
            File.Delete(DatabaseFile);
        }
    }
}