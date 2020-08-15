using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NubeSync.Core;

namespace NubeSync.Client.SQLiteStore
{
    public partial class NubeSQLiteDataStore
    {
        public async Task<IQueryable<T>> AllAsync<T>() where T : NubeTable, new()
        {
            return (await Database.Table<T>().ToListAsync()).AsQueryable();
        }

        public async Task<bool> DeleteAsync<T>(T item) where T : NubeTable, new()
        {
            return await Database.DeleteAsync(item) > 0;
        }

        public async Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable, new()
        {
            return (await Database.Table<T>().Where(predicate).ToListAsync()).AsQueryable();
        }

        public async Task<T> FindByIdAsync<T>(string? id) where T : NubeTable?, new()
        {
            return await Database.FindAsync<T>(id);
        }

        public async Task<bool> InsertAsync<T>(T item) where T : NubeTable, new()
        {
            return await Database.InsertAsync(item) > 0;
        }

        public Task<bool> TableExistsAsync<T>() where T : NubeTable, new()
        {
            return Task.FromResult(Database.TableMappings.Any(m => m.MappedType == typeof(T)));
        }

        public async Task<bool> UpdateAsync<T>(T item) where T : NubeTable, new()
        {
            return await Database.UpdateAsync(item) > 0;
        }
    }
}