﻿using System.Collections.Generic;
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

            return await Database.InsertAllAsync(operations).ConfigureAwait(false) > 0;
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
                deletedRecords += await Database.DeleteAsync(operation).ConfigureAwait(false);
            }

            return deletedRecords > 0;
        }

        public async Task<IQueryable<NubeOperation>> GetOperationsAsync(int numberOfOperations = 0)
        {
            if (numberOfOperations == 0)
            {
                return (await Database.Table<NubeOperation>().OrderBy(o => o.CreatedAt).ToListAsync().ConfigureAwait(false)).AsQueryable();
            }

            var groupedOperations = (await Database.Table<NubeOperation>().OrderBy(o => o.CreatedAt).ToListAsync())
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

            return operations.AsQueryable();
        }
    }
}