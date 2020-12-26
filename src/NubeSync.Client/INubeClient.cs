using NubeSync.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NubeSync.Client
{
    public interface INubeClient
    {
        /// <summary>
        /// Registers a table to be handled by the NubeSync client. All tables that should be synced have to be registered this way.
        /// </summary>
        /// <typeparam name="T">The type of the table to be synced.</typeparam>
        /// <param name="tableUrl">Optional: The url to the table controller on the server, if left empty the name of the type will be used.</param>
        Task AddTableAsync<T>(string? tableUrl = null) where T : NubeTable;

        /// <summary>
        /// Deletes the given record from the local storage and adds the generated sync operations.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <param name="disableChangeTracker">Optional: If true generating the sync operations is omitted.</param>
        Task DeleteAsync<T>(T item, bool disableChangeTracker = false) where T : NubeTable;

        /// <summary>
        /// Queries the local storage with the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of the table to be queried.</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>All items from the table matching the criteria.</returns>
        Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable;

        /// <summary>
        /// Returns all items from the table.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <returns>All items from the table.</returns>
        Task<IQueryable<T>> GetAllAsync<T>() where T : NubeTable;

        /// <summary>
        /// Gets a item by its id.
        /// </summary>
        /// <typeparam name="T">The type of the table to be queried.</typeparam>
        /// <param name="id">The id of the item.</param>
        /// <returns>The item with the given id.</returns>
        Task<T> GetByIdAsync<T>(string id) where T : NubeTable;

        /// <summary>
        /// Checks wether there are any sync operations that have not be pushed to the server.
        /// </summary>
        /// <returns>True if there are unsent operations in the local storage.</returns>
        Task<bool> HasPendingOperationsAsync();

        /// <summary>
        /// Pulls all changes for this table from the server.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <param name="cancelToken">Optional: A token for canceling the operation.</param>
        /// <returns>The number of records that were changed or added in this table since the last sync.</returns>
        /// <exception cref="PullOperationFailedException">Thrown when the table cannot be pulled from the server.</exception>
        Task<int> PullTableAsync<T>(CancellationToken cancelToken = default) where T : NubeTable;

        /// <summary>
        /// Pushes the changes made in the local storage to the server.
        /// </summary>
        /// <param name="cancelToken">Optional: A token for canceling the operation.</param>
        /// <returns>True if the push operation was successful.</returns>
        /// <exception cref="PushOperationFailedException">Thrown when the changes cannot be pushed to the server.</exception>
        Task<bool> PushChangesAsync(CancellationToken cancelToken = default);

        /// <summary>
        /// Saves a record to the local storage. Adds it if it does not exist or updates it, if a record with the given id already exists. Also adds all sync operations for the changes made.
        /// </summary>
        /// <param name="item">The item to be saved.</param>
        /// <param name="existingItem">The item before any changes were made to is (= the version of the element that is currently stored in the database).</param>
        /// <param name="disableChangeTracker">Optional: If true generating the sync operations is omitted.</param>
        Task SaveAsync<T>(T item, T? existingItem = null, bool disableChangeTracker = false) where T : NubeTable;
    }
}