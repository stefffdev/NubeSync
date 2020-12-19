using NubeSync.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NubeSync.Client
{
    public partial class NubeClient
    {
        private const int OPERATIONS_PAGE_SIZE = 100;
        private const int PULL_PAGE_SIZE = 100;
        private bool _isSyncing;

        /// <summary>
        /// Checks wether there are any sync operations that have not be pushed to the server.
        /// </summary>
        /// <returns>True if there are unsent operations in the local storage.</returns>
        public async Task<bool> HasPendingOperationsAsync()
        {
            return (await _dataStore.GetOperationsAsync().ConfigureAwait(false)).Count() > 0;
        }

        /// <summary>
        /// Pulls all changes for this table from the server.
        /// </summary>
        /// <typeparam name="T">The type of the table.</typeparam>
        /// <param name="cancelToken">Optional: A token for canceling the operation.</param>
        /// <returns>The number of records that were changed or added in this table since the last sync.</returns>
        /// <exception cref="PullOperationFailedException">Thrown when the table cannot be pulled from the server.</exception>
        public async Task<int> PullTableAsync<T>(CancellationToken cancelToken = default) where T : NubeTable
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
                var timestampParameter = await _GetTimestampParameter(tableName).ConfigureAwait(false);

                await _SetInstallationId();

                var processedRecords = 0;
                var pageNumber = 1;
                var items = new List<T>();

                do
                {
                    await _AuthenticateAsync().ConfigureAwait(false);

                    var parameters = $"?pageNumber={pageNumber}&pageSize={PULL_PAGE_SIZE}{timestampParameter}";

                    var result = await _httpClient.GetAsync($"/{_nubeTableTypes[tableName].Trim('/')}{parameters}", cancelToken).ConfigureAwait(false);

                    if (!result.IsSuccessStatusCode)
                    {
                        throw new PullOperationFailedException($"{result.StatusCode}");
                    }

                    var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    items = JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    foreach (var item in items)
                    {
                        await _ProcessItem(content, item).ConfigureAwait(false);
                    }

                    processedRecords += items.Count;
                    pageNumber++;
                } while (items.Any() && items.Count == PULL_PAGE_SIZE);
                // cancelling with different page size for the case when the server does not implement paging

                await _SetLastSyncTimestampAsync(tableName).ConfigureAwait(false);
                return processedRecords;
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
                var operations = await _dataStore.GetOperationsAsync(OPERATIONS_PAGE_SIZE).ConfigureAwait(false);

                while (operations.Any())
                {
                    await _AuthenticateAsync().ConfigureAwait(false);
                    await _SetInstallationId();

                    var options = new JsonSerializerOptions { IgnoreNullValues = true };
                    var content = new StringContent(JsonSerializer.Serialize(operations, options),
                        Encoding.UTF8, "application/json");
                    var result = await _httpClient.PostAsync("/operations", content, cancelToken).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        await _dataStore.DeleteOperationsAsync(operations.ToArray()).ConfigureAwait(false);
                    }
                    else
                    {
                        var message = string.Empty;

                        try
                        {
                            message = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        }
                        catch (Exception) { }

                        throw new PushOperationFailedException($"Cannot push operations to the server: {result.StatusCode} {message}");
                    }

                    operations = await _dataStore.GetOperationsAsync(OPERATIONS_PAGE_SIZE).ConfigureAwait(false);
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
                var token = await _authentication.GetBearerTokenAsync().ConfigureAwait(false);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<DateTimeOffset?> _GetLastSyncTimestampAsync(string tableName)
        {
            DateTimeOffset? result = null;

            if (DateTimeOffset.TryParse(await _dataStore.GetSettingAsync($"lastSync-{tableName}").ConfigureAwait(false), out var lastSync))
            {
                result = lastSync;
            }

            return result;
        }

        private async Task<string> _GetTimestampParameter(string tableName)
        {
            var lastSync = await _GetLastSyncTimestampAsync(tableName).ConfigureAwait(false);
            if (lastSync.HasValue)
            {
                return $"&laterThan={lastSync.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}";
            }

            return string.Empty;
        }

        private bool _IsItemDeleted(string content, string itemId)
        {
            if (JsonDocument.Parse(content).RootElement.EnumerateArray()
                .First(x => x.GetProperty("id").GetString() == itemId).TryGetProperty("deletedAt", out var deletedAt))
            {
                return !string.IsNullOrEmpty(deletedAt.GetString());
            }

            return false;
        }

        private async Task _ProcessItem<T>(string content, T item) where T : NubeTable
        {
            if (_IsItemDeleted(content, item.Id))
            {
                var deleteItem = await _dataStore.FindByIdAsync<T>(item.Id).ConfigureAwait(false);
                if (deleteItem != null)
                {
                    await DeleteAsync(deleteItem, disableChangeTracker: true).ConfigureAwait(false);
                }
            }
            else
            {
                await SaveAsync(item, disableChangeTracker: true).ConfigureAwait(false);
            }
        }

        private async Task _SetInstallationId()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains(INSTALLATION_ID_HEADER))
            {
                var installationIdKey = "installationId";
                var id = await _dataStore.GetSettingAsync(installationIdKey);
                if (id == null || string.IsNullOrWhiteSpace(id))
                {
                    id = Guid.NewGuid().ToString();
                    await _dataStore.SetSettingAsync(installationIdKey, id);
                }

                _httpClient.DefaultRequestHeaders.Add(INSTALLATION_ID_HEADER, id);
            }
        }

        private async Task _SetLastSyncTimestampAsync(string tableName)
        {
            await _dataStore.SetSettingAsync($"lastSync-{tableName}", DateTimeOffset.Now.ToString("o")).ConfigureAwait(false);
        }
    }
}