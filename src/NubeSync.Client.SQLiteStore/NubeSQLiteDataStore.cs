using System.Threading.Tasks;
using NubeSync.Core;
using SQLite;

namespace NubeSync.Client.SQLiteStore
{
    public partial class NubeSQLiteDataStore : IDataStore
    {
        public readonly SQLiteAsyncConnection Database;

        public NubeSQLiteDataStore(string databasePath = "nube-offline.db")
        {
            Database = new SQLiteAsyncConnection(databasePath);
        }

        public async Task AddTableAsync<T>() where T : NubeTable, new()
        {
            await Database.CreateTableAsync<T>(CreateFlags.ImplicitPK).ConfigureAwait(false);
        }

        public async Task InitializeAsync()
        {
            await Database.CreateTableAsync<NubeOperation>(CreateFlags.ImplicitPK).ConfigureAwait(false);
            await Database.CreateIndexAsync(nameof(NubeOperation), nameof(NubeOperation.CreatedAt)).ConfigureAwait(false);

            await Database.CreateTableAsync<NubeSetting>(CreateFlags.ImplicitPK).ConfigureAwait(false);
        }
    }
}