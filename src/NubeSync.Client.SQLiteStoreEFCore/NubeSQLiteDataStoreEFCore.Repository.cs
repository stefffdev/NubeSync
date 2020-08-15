using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Core;

namespace NubeSync.Client.SQLiteStoreEFCore
{
    public partial class NubeSQLiteDataStoreEFCore
    {
        public Task<IQueryable<T>> AllAsync<T>() where T : NubeTable, new()
        {
            return Task.FromResult(Set<T>().AsNoTracking().AsQueryable());
        }

        public async Task<bool> DeleteAsync<T>(T item) where T : NubeTable, new()
        {
            var dbItem = await Set<T>().FindAsync(item.Id);
            Set<T>().Remove(dbItem);
            return await SaveChangesAsync() > 0;
        }

        public Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable, new()
        {
            return Task.FromResult(Set<T>().AsNoTracking().Where(predicate));
        }

        public async Task<T> FindByIdAsync<T>(string? id) where T : NubeTable?, new()
        {
            var entity = await Set<T>().FindAsync(id);
            if (entity != null)
            {
                Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }

        public async Task<bool> InsertAsync<T>(T item) where T : NubeTable, new()
        {
            await Set<T>().AddAsync(item);
            return await SaveChangesAsync() > 0;
        }

        public Task<bool> TableExistsAsync<T>() where T : NubeTable, new()
        {
            try
            {
                Set<T>().Count();
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> UpdateAsync<T>(T item) where T : NubeTable, new()
        {
            Set<T>().Attach(item);
            Entry(item).State = EntityState.Modified;
            return await SaveChangesAsync() > 0;
        }
    }
}