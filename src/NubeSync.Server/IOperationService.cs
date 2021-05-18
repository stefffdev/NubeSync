using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Core;
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
        /// <param name="userId">The is of the user that processes the operations.</param>
        /// <param name="installationId">The installationId that processes the operations.</param>
        /// <returns>A list of all records that were created or changed and the OperationType that caused the change.</returns>
        Task<List<(NubeServerTable Entity, OperationType Type)>> ProcessOperationsAsync(DbContext context, IList<NubeOperation> operations, string userId = "", string installationId = "");
    }
}