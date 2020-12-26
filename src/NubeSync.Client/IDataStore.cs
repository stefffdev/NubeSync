using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NubeSync.Core;

namespace NubeSync.Client
{
    public interface IDataStore
    {
        /// <summary>
        /// Persists the provided operations in the local storage.
        /// </summary>
        /// <param name="operations">The operations to be added.</param>
        /// <returns>If the storage operation was successful.</returns>
        Task<bool> AddOperationsAsync(params NubeOperation[] operations);

        /// <summary>
        /// Registers a table within the local storage provider.
        /// All tables that should be handled must be registered this way.
        /// </summary>
        /// <typeparam name="T">The type of the table to be registered.</typeparam>
        Task AddTableAsync<T>(string? tableUrl = null) where T : NubeTable;

        /// <summary>
        /// Returns all records in the table of type T.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <returns>All records in that table.</returns>
        Task<IQueryable<T>> AllAsync<T>() where T : NubeTable;

        /// <summary>
        /// Deletes the item from the local storage.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <returns>If the removal ot the item was successful.</returns>
        Task<bool> DeleteAsync<T>(T item) where T : NubeTable;

        /// <summary>
        /// Deletes the provided operations from the local storage.
        /// </summary>
        /// <param name="operations">The operations to  be deleted.</param>
        /// <returns>If the removal of the operation was successful.</returns>
        Task<bool> DeleteOperationsAsync(params NubeOperation[] operations);

        /// <summary>
        /// Queries the local storage by the predicate for items of type T.
        /// </summary>
        /// <typeparam name="T">The table type.</typeparam>
        /// <param name="predicate">The predicate for the query.</param>
        /// <returns>The records from the table matching the provided criteria.</returns>
        Task<IQueryable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate) where T : NubeTable;

        /// <summary>
        /// Find a specific item by its id.
        /// </summary>
        /// <typeparam name="T">The table type.</typeparam>
        /// <param name="id">The id of the item.</param>
        /// <returns>The item matching the provided id.</returns>
        Task<T> FindByIdAsync<T>(string? id) where T : NubeTable?;

        /// <summary>
        /// Get all operations from the local storage.
        /// </summary>
        /// <param name="numberOfOperations">The number of operations to be retrieved. If left empty all operations will be returned.</param>
        /// <returns>The operations.</returns>
        Task<IQueryable<NubeOperation>> GetOperationsAsync(int numberOfOperations = 0);

        /// <summary>
        /// Reads the value of a setting from the local storage.
        /// </summary>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The value for the key provided or null if the key was not found in the settings storage.</returns>
        Task<string?> GetSettingAsync(string key);

        /// <summary>
        /// Inserts a record into the local storage.
        /// </summary>
        /// <param name="item">The record to be inserted.</param>
        /// <returns>If the insertion was successful.</returns>
        Task<bool> InsertAsync<T>(T item) where T : NubeTable;

        /// <summary>
        /// Sets the value of a setting in the local storage.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The value to be stored.</param>
        /// <returns>If saving the setting succeeded.</returns>
        Task<bool> SetSettingAsync(string key, string value);

        /// <summary>
        /// Checks whether a table of the given type exists in the local storage.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <returns>True if the table can be found in the local storage.</returns>
        Task<bool> TableExistsAsync<T>() where T : NubeTable;

        /// <summary>
        /// Updates the given record in the local storage.
        /// </summary>
        /// <param name="item">The record to be stored.</param>
        /// <returns>If storing the record was successful.</returns>
        Task<bool> UpdateAsync<T>(T item) where T : NubeTable;
    }
}