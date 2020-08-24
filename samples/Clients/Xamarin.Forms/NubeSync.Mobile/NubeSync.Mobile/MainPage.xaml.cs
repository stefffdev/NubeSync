using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NubeSync.Client;
using NubeSync.Client.SQLiteStore;
using SQLite;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NubeSync.Mobile
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly NubeSQLiteDataStore _dataStore;
        private readonly NubeClient _nubeClient;

        public MainPage()
        {
            InitializeComponent();

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "offline.db");
            _dataStore = new NubeSQLiteDataStore(databasePath);

            // this is needed when the server is running local for debugging
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            var httpClient = new HttpClient(clientHandler);

            var server = "https://localhost:5001/";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                server = "https://10.0.2.2:5001/";
            }

            _nubeClient = new NubeClient(_dataStore, server, httpClient: httpClient);
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await _dataStore.InitializeAsync();
            await _nubeClient.AddTableAsync<TodoItem>("todoitems");
            
            await RefreshItemsAsync();

            // this is an optional step to enable "live push/pull"
            // see https://github.com/stefffdev/NubeSync/wiki/Advanced:-Live-updates-with-SignalR
            // await ConfigureSignalRAsync();
        }

        private async Task RefreshItemsAsync()
        {
            var list = (await _nubeClient.GetAllAsync<TodoItem>()).ToList().OrderBy(i => i.CreatedAt);
            collectionView.ItemsSource = list;
        }
        
        private async Task SyncAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                return;
            }

            await _nubeClient.PushChangesAsync();
            await _nubeClient.PullTableAsync<TodoItem>();

            await RefreshItemsAsync();
        }

        private async void Add_Button_Clicked(object sender, EventArgs e)
        {
            var item = new TodoItem() { Name = "New Item" };
            await _nubeClient.SaveAsync(item);
            await RefreshItemsAsync();
        }

        private async void Delete_Button_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TodoItem item)
            {
                await _nubeClient.DeleteAsync(item);
                await RefreshItemsAsync();
            }
        }

        private async void Sync_Button_Clicked(object sender, EventArgs e)
        {
            await SyncAsync();
        }

        private async void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is TodoItem item)
            {
                item.Name = entry.Text;
                await _nubeClient.SaveAsync(item);
                await _nubeClient.PushChangesAsync();
            }
        }

        private async Task ConfigureSignalRAsync()
        {
            string hubUrl = "https://localhost:5001/updateHub";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                hubUrl = "https://10.0.2.2:5001/updateHub";
            }

            var hubConnection = new HubConnectionBuilder()
               .WithUrl(hubUrl, (opts) =>
               {
                   opts.HttpMessageHandlerFactory = (message) =>
                   {
                       if (message is HttpClientHandler clientHandler2)
                            // bypass the SSL certificate for local debugging
                            clientHandler2.ServerCertificateCustomValidationCallback +=
                               (sender, certificate, chain, sslPolicyErrors) => { return true; };
                       return message;
                   };
               })
               .Build();

            hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await hubConnection.StartAsync();
            };

            hubConnection.On<string, string>("Update", async (user, message) =>
            {
                await SyncAsync();
            });

            await hubConnection.StartAsync();
        }
    }
}
