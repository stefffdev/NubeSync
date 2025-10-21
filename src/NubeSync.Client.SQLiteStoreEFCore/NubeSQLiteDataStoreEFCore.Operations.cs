namespace NubeSync.Client.SQLiteStoreEFCore;

public partial class NubeSQLiteDataStoreEFCore
{
    public async Task<bool> AddOperationsAsync(params NubeOperation[] operations)
    {
        if (operations.Length == 0)
        {
            return true;
        }

        await NubeOperations.AddRangeAsync(operations).ConfigureAwait(false);
        return await SaveChangesAsync().ConfigureAwait(false) > 0;
    }

    public async Task<bool> DeleteOperationsAsync(params NubeOperation[] operations)
    {
        if (operations.Length == 0)
        {
            return true;
        }

        var deletedRecords = 0;

        await using (var transaction = await Database.BeginTransactionAsync())
        {
            foreach (var operation in operations)
            {
                var entity = await NubeOperations.FindAsync(operation.Id).ConfigureAwait(false);
                if (entity == null)
                {
                    continue;
                }
                
                NubeOperations.Remove(entity);
                deletedRecords += await SaveChangesAsync().ConfigureAwait(false);
            }

            await transaction.CommitAsync();
        }

        return deletedRecords > 0;
    }

    public Task<IQueryable<NubeOperation>> GetOperationsAsync(int numberOfOperations = 0)
    {
        if (numberOfOperations == 0)
        {
            return Task.FromResult(NubeOperations.ToList().OrderBy(o => o.CreatedAt).AsQueryable());
        }

        var groupedOperations = NubeOperations.ToList().OrderBy(o => o.CreatedAt)
            .GroupBy(o => new { o.TableName, o.ItemId });

        var operations = new List<NubeOperation>();

        foreach (var group in groupedOperations)
        {
            if (operations.Count + group.Count() > numberOfOperations && operations.Any())
            {
                break;
            }

            operations.AddRange(group);
        }

        return Task.FromResult(operations.AsQueryable());
    }
}