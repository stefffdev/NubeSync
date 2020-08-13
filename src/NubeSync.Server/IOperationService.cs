using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Server.Data;

namespace NubeSync.Server
{
    public interface IOperationService
    {
        /// <summary>
        /// Checks if the last change of the record was done by another user (and therefore has to be downloaded when performing a sync).
        /// </summary>
        /// <param name="context">The DbContext for accessing the database.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="itemId">The id of the item to be checked.</param>
        /// <param name="installationId">The installation id of the client.</param>
        /// <param name="laterThan">The timestamp of the last sync.</param>
        /// <returns>True when the latest changes were made by another installation id.</returns>
        bool LastChangedByOthers(DbContext context, string tableName, string itemId, string installationId, DateTimeOffset laterThan);

        /// <summary>
        /// Processes the given operations and updates the database accordingly.
        /// </summary>
        /// <param name="context">The DbContext for accessing the database.</param>
        /// <param name="operations">The operations to be processed.</param>
        Task ProcessOperationsAsync(DbContext context, IList<NubeOperation> operations);
    }
}