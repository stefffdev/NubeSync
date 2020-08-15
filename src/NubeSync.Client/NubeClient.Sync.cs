using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NubeSync.Client.Helpers;
using NubeSync.Core;

namespace NubeSync.Client
{
    public partial class NubeClient
    {
        private const int OPERATIONS_PAGE_SIZE = 100;
        private bool _isSyncing;

        /// <summary>
        /// Checks wether there are any sync operations that have not be pushed to the server.
        /// </summary>
        /// <returns>True if there are unsent operations in the local storage.</returns>
        public async Task<bool> HasPendingOperationsAsync()
        {
            return (await _dataStore.GetOperationsAsync()).Count() > 0;
        }

        /// <summary>
        /// Pulls all changes for this table from the server.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <param name="cancelToken">Optional: A token for canceling the operation.</param>
        /// <returns>The number of records that were changed or added in this table since the last sync.</returns>
        /// <exception cref="PullOperationFailedException">Thrown when the table cannot be pulled from the server.</exception>
        public async Task<int> PullTableAsync<T>(CancellationToken cancelToken = default) where T : NubeTable, new()
        {
            if (_isSyncing || cancelToken.IsCancellationRequested)
            {
                return 0;
            }
            _isSyncing = true;

            try
            {
                _IsValidTable<T>();

                var tableName = typeof(T).Name;
                var parameters = string.Empty;

                var lastSync = await _GetLastSyncTimestampAsync(tableName);
                if (lastSync.HasValue)
                {
                    parameters = $"?laterThan={lastSync.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}";
                }

                await _AuthenticateAsync();

                var result = await _httpClient.GetAsync($"/{_nubeTableTypes[tableName].Trim('/')}{parameters}", cancelToken);
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();

                    var items = JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    foreach (var item in items)
                    {
                        if (_IsItemDeleted(content, item.Id))
                        {
                            var deleteItem = await _dataStore.FindByIdAsync<T>(item.Id);
                            if (deleteItem != null)
                            {
                                await DeleteAsync(deleteItem, disableChangeTracker: true);
                            }
                        }
                        else
                        {
                            var localItem = await _dataStore.FindByIdAsync<T>(item.Id);
                            if (localItem == null)
                            {
                                localItem = Activator.CreateInstance<T>();
                                if (localItem == null)
                                {
                                    throw new PullOperationFailedException($"Cannot create item of type {tableName}");
                                }

                                localItem.Id = item.Id;
                            }

                            ObjectHelper.CopyProperties(item, localItem);
                            await SaveAsync(localItem, disableChangeTracker: true);
                        }
                    }

                    await _SetLastSyncTimestampAsync(tableName);
                    return items.Count;
                }
                else
                {
                    throw new PullOperationFailedException($"{result.StatusCode}");
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        /// <summary>
        /// Pushes the changes made in the local storage to the server.
        /// </summary>
        /// <param name="cancelToken">Optional: A token for canceling the operation.</param>
        /// <returns>True if the push operation was successful.</returns>
        /// <exception cref="PushOperationFailedException">Thrown when the changes cannot be pushed to the server.</exception>
        public async Task<bool> PushChangesAsync(CancellationToken cancelToken = default)
        {
            if (_isSyncing || cancelToken.IsCancellationRequested)
            {
                return false;
            }
            _isSyncing = true;

            try
            {
                var operations = await _dataStore.GetOperationsAsync(OPERATIONS_PAGE_SIZE);
                
                while (operations.Any())
                {
                    await _AuthenticateAsync();

                    var options = new JsonSerializerOptions { IgnoreNullValues = true };
                    var content = new StringContent(JsonSerializer.Serialize(operations, options),
                        Encoding.UTF8, "application/json");
                    var result = await _httpClient.PostAsync("/operations", content, cancelToken);

                    if (result.IsSuccessStatusCode)
                    {
                        await _dataStore.DeleteOperationsAsync(operations.ToArray());
                    }
                    else
                    {
                        var message = string.Empty;

                        try
                        {
                            message = await result.Content.ReadAsStringAsync();
                        }
                        catch (Exception)
                        {
                        }

                        throw new PushOperationFailedException($"Cannot push operations to the server: {result.StatusCode} {message}");
                    }

                    operations = await _dataStore.GetOperationsAsync(OPERATIONS_PAGE_SIZE);
                }
            }
            finally
            {
                _isSyncing = false;
            }

            return true;
        }

        private async Task _AuthenticateAsync()
        {
            if (_authentication != null)
            {
                var token = await _authentication.GetBearerTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<DateTimeOffset?> _GetLastSyncTimestampAsync(string tableName)
        {
            DateTimeOffset? result = null;

            if (DateTimeOffset.TryParse(await _dataStore.GetSettingAsync($"lastSync-{tableName}"), out var lastSync))
            {
                result = lastSync;
            }

            return result;
        }

        private bool _IsItemDeleted(string content, string itemId)
        {
            if (JsonDocument.Parse(content).RootElement.EnumerateArray()
                .First(x => x.GetProperty("id").GetString() == itemId).TryGetProperty("deletedAt", out var deletedAt ))
            {
                return !string.IsNullOrEmpty(deletedAt.GetString());
            }

            return false;
        }

        private async Task _SetLastSyncTimestampAsync(string tableName)
        {
            await _dataStore.SetSettingAsync($"lastSync-{tableName}", DateTimeOffset.Now.ToString());
        }
    }
}