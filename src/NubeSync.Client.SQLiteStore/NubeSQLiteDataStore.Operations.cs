using System.Linq;
using System.Threading.Tasks;
using NubeSync.Core;

namespace NubeSync.Client.SQLiteStore
{
    public partial class NubeSQLiteDataStore
    {
        public async Task<bool> AddOperationsAsync(params NubeOperation[] operations)
        {
            if (operations == null || !operations.Any())
            {
                return true;
            }

            return await Database.InsertAllAsync(operations) > 0;
        }

        public async Task<bool> DeleteOperationsAsync(params NubeOperation[] operations)
        {
            if (operations == null || !operations.Any())
            {
                return true;
            }

            var deletedRecords = 0;

            foreach (var operation in operations)
            {
                deletedRecords += await Database.DeleteAsync(operation);
            }

            return deletedRecords > 0;
        }

        public async Task<IQueryable<NubeOperation>> GetOperationsAsync(int numberOfOperations = 0)
        {
            if (numberOfOperations != 0)
            {
                return (await Database.Table<NubeOperation>().OrderBy(o => o.CreatedAt).Take(numberOfOperations).ToListAsync()).AsQueryable();
            }

            return (await Database.Table<NubeOperation>().OrderBy(o => o.CreatedAt).ToListAsync()).AsQueryable();
        }
    }
}