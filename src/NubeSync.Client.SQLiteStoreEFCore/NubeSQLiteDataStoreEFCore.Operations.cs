using System.Linq;
using System.Threading.Tasks;
using NubeSync.Client.Data;

namespace NubeSync.Client.SQLiteStoreEFCore
{
    public partial class NubeSQLiteDataStoreEFCore
    {
        public async Task<bool> AddOperationsAsync(params NubeOperation[] operations)
        {
            if (operations == null || !operations.Any())
            {
                return true;
            }

            await NubeOperations.AddRangeAsync(operations);
            return await SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteOperationsAsync(params NubeOperation[] operations)
        {
            if (operations == null || !operations.Any())
            {
                return true;
            }

            var deletedRecords = 0;

            using (var transaction = Database.BeginTransaction())
            {
                foreach (var operation in operations)
                {
                    var entity = await NubeOperations.FindAsync(operation.Id);
                    NubeOperations.Remove(entity);
                    deletedRecords += await SaveChangesAsync();
                }

                transaction.Commit();
            }

            return deletedRecords > 0;
        }

        public Task<IQueryable<NubeOperation>> GetOperationsAsync(int numberOfOperations = 0)
        {
            if (numberOfOperations != 0)
            {
                return Task.FromResult(NubeOperations.ToList().OrderBy(o => o.CreatedAt).Take(numberOfOperations).AsQueryable());
            }

            return Task.FromResult(NubeOperations.ToList().OrderBy(o => o.CreatedAt).AsQueryable());
        }
    }
}