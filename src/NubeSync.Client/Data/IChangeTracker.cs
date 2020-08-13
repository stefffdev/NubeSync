using System.Collections.Generic;
using System.Threading.Tasks;

namespace NubeSync.Client.Data
{
    public interface IChangeTracker
    {
        /// <summary>
        /// Creates the according operations for a record that was added to the datastore.
        /// </summary>
        /// <typeparam name="T">The type of the added record</typeparam>
        /// <param name="item">The record that was added</param>
        /// <returns>The created operations</returns>
        Task<List<NubeOperation>> TrackAddAsync<T>(T item) where T : NubeTable;

        /// <summary>
        /// Creates the according operations for a record that was deleted from the datastore.
        /// </summary>
        /// <typeparam name="T">The type of the deleted record</typeparam>
        /// <param name="item">The record that was deleted</param>
        /// <returns>The created operations</returns>
        Task<NubeOperation> TrackDeleteAsync<T>(T item) where T : NubeTable;

        /// <summary>
        /// Creates the according operations for a record that was modified in the datastore.
        /// </summary>
        /// <typeparam name="T">The type of the modified record</typeparam>
        /// <param name="oldItem">The original record before the modifications where performed.</param>
        /// <param name="newItem">The current record containing the changes made.</param>
        /// <returns>The created operations</returns>
        Task<List<NubeOperation>> TrackModifyAsync<T>(T oldItem, T newItem) where T : NubeTable;
    }
}