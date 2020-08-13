using System;
using System.IO;
using NubeSync.Client.Data;
using NubeSync.Client.SQLiteStore;

namespace Tests.NubeSync.Client.SQLiteStore
{
    public class NubeSQLiteDataStoreTestBase : IDisposable
    {
        protected string DatabaseFile;
        protected NubeSQLiteDataStore DataStore;
        protected NubeOperation[] Operations;

        public NubeSQLiteDataStoreTestBase()
        {
            DatabaseFile = Guid.NewGuid().ToString();
            DataStore = new NubeSQLiteDataStore(DatabaseFile);

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
            DataStore.Database.CloseAsync().Wait();
            File.Delete(DatabaseFile);
        }
    }
}