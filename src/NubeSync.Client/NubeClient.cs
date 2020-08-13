using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NubeSync.Client.Data;

namespace NubeSync.Client
{
    public partial class NubeClient
    {
        private readonly INubeAuthentication? _authentication;
        private readonly IChangeTracker _changeTracker;
        private readonly INubeClientConfiguration _configuration;
        private readonly IDataStore _dataStore;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _nubeTableTypes;

        /// <summary>
        /// Creates a instance of the NubeSync client
        /// </summary>
        /// <param name="dataStore">The local storage</param>
        /// <param name="configuration">Configuration containing the sync server</param>
        /// <param name="authentication">Optional: a authentication provider, if the server requires the requests to be authenticated.</param>
        /// <param name="httpClient">Optional: the HttpClient that is used for communicating with the server.</param>
        /// <param name="changeTracker">Optional: the change tracker generating the operations.</param>
        /// <param name="installationId">Optional: a unique installation id, see https://github.com/stefffdev/NubeSync/wiki/Advanced:-Don't-download-unnecessary-records-when-syncing</param>
        public NubeClient(
            IDataStore dataStore,
            INubeClientConfiguration configuration,
            INubeAuthentication? authentication = null,
            HttpClient? httpClient = null,
            IChangeTracker? changeTracker = null,
            string? installationId = null)
        {
            _dataStore = dataStore;
            _configuration = configuration;
            _authentication = authentication;
            _httpClient = httpClient ?? new HttpClient();
            _changeTracker = changeTracker ?? new ChangeTracker();

            _nubeTableTypes = new Dictionary<string, string>();
            _httpClient.BaseAddress = new Uri(_configuration.Server);

            if (!string.IsNullOrEmpty(installationId))
            {
                _httpClient.DefaultRequestHeaders.Add("NUBE-INSTALLATION-ID", installationId);
            }
        }

        /// <summary>
        /// Registers a table to be handled by the NubeSync client. All tables that should be synced have to be registered this way.
        /// </summary>
        /// <typeparam name="T">The type of the table to be synced.</typeparam>
        /// <param name="tableUrl">Optional: The url to the table controller on the server, if left empty the name of the type will be used.</param>
        public async Task AddTableAsync<T>(string? tableUrl = null) where T : NubeTable, new()
        {
            await _dataStore.AddTableAsync<T>();

            if (!await _dataStore.TableExistsAsync<T>())
            {
                throw new ArgumentException($"The table type {typeof(T).Name} cannot be found in the data store");
            }

            tableUrl ??= "/" + typeof(T).Name;
            _nubeTableTypes.Add(typeof(T).Name, tableUrl);
        }
    }
}