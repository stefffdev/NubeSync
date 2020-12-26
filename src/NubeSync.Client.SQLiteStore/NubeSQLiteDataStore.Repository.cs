using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NubeSync.Core;
using SQLite;

namespace NubeSync.Client.SQLiteStore
{
    public partial class NubeSQLiteDataStore
    {
        public async Task<IQueryable<T>> AllAsync<T>() where T : NubeTable
        {
            return await Task.Factory.StartNew(() => {
                var conn = Database.GetConnection();
                using (conn.Lock())
                {
                    var tableQuery = new TableQuery<T>(Database.GetConnection());
                    return tableQuery.AsQueryable();
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).ConfigureAwait
(false);
        }

        public async Task<bool> DeleteAsync<T>(T item) where T : NubeTable
        {
            return await Database.DeleteAsync(item).ConfigureAwait(false) > 0;
        }

        public async Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable
        {
            return await Task.Factory.StartNew(() => {
                var conn = Database.GetConnection();
                using (conn.Lock())
                {
                    var tableQuery = new TableQuery<T>(Database.GetConnection());
                    return tableQuery.Where(predicate).AsQueryable();
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).ConfigureAwait(false);
        }

        public async Task<T> FindByIdAsync<T>(string? id) where T : NubeTable?
        {
            var mapping = await _GetMappingAsync<T>();
            return (T) await Database.FindAsync(id, mapping).ConfigureAwait(false);
        }

        public async Task<bool> InsertAsync<T>(T item) where T : NubeTable
        {
            return await Database.InsertAsync(item).ConfigureAwait(false) > 0;
        }

        public Task<bool> TableExistsAsync<T>() where T : NubeTable
        {
            return Task.FromResult(Database.TableMappings.Any(m => m.MappedType == typeof(T)));
        }

        public async Task<bool> UpdateAsync<T>(T item) where T : NubeTable
        {
            return await Database.UpdateAsync(item).ConfigureAwait(false) > 0;
        }

        private async Task<TableMapping> _GetMappingAsync<T>()
        {
            return await Database.GetMappingAsync(typeof(T), CreateFlags.ImplicitPK).ConfigureAwait(false);
        }
    }
}