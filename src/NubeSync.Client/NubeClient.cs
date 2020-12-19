using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NubeSync.Core;

namespace NubeSync.Client
{
    public partial class NubeClient : INubeClient
    {
        private const string INSTALLATION_ID_HEADER = "NUBE-INSTALLATION-ID";
        private readonly INubeAuthentication? _authentication;
        private readonly IChangeTracker _changeTracker;
        private readonly IDataStore _dataStore;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _nubeTableTypes;

        /// <summary>
        /// Creates a instance of the NubeSync client
        /// </summary>
        /// <param name="dataStore">The local storage</param>
        /// <param name="url">The address of the server hosting the NubeSync REST APIs</param>
        /// <param name="authentication">Optional: a authentication provider, if the server requires the requests to be authenticated.</param>
        /// <param name="httpClient">Optional: the HttpClient that is used for communicating with the server.</param>
        /// <param name="changeTracker">Optional: the change tracker generating the operations.</param>
        public NubeClient(
            IDataStore dataStore,
            string url,
            INubeAuthentication? authentication = null,
            HttpClient? httpClient = null,
            IChangeTracker? changeTracker = null)
        {
            _dataStore = dataStore;
            _authentication = authentication;
            _httpClient = httpClient ?? new HttpClient();
            _changeTracker = changeTracker ?? new ChangeTracker();

            _nubeTableTypes = new Dictionary<string, string>();
            _httpClient.BaseAddress = new Uri(url);
        }

        public async Task AddTableAsync<T>(string? tableUrl = null) where T : NubeTable
        {
            await _dataStore.AddTableAsync<T>().ConfigureAwait(false);

            if (!await _dataStore.TableExistsAsync<T>().ConfigureAwait(false))
            {
                throw new ArgumentException($"The table type {typeof(T).Name} cannot be found in the data store");
            }

            tableUrl ??= "/" + typeof(T).Name;
            _nubeTableTypes.Add(typeof(T).Name, tableUrl);
        }
    }
}