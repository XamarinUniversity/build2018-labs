using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MyCircle.Services
{
    public sealed class AzureMessageRepository : IAsyncMessageRepository
    {
        const string AzureServiceUrl = "https://build2018mycircle.azurewebsites.net";
        MobileServiceClient client;
        IMobileServiceSyncTable<CircleMessage> messages;

        public AzureMessageRepository()
        {
            client = new MobileServiceClient(AzureServiceUrl);
        }

        async Task InitializeTableAsync()
        {
            if (messages == null)
            {
                await InitializeOfflineStorageAsync();
                messages = client.GetSyncTable<CircleMessage>();
                await PurgeOldRecordsAsync(/*true*/);
            }
        }

        private async Task PurgeOldRecordsAsync(bool clearLocalCache = false)
        {
            if (!clearLocalCache)
            {
                var oneWeek = DateTimeOffset.Now.AddDays(-7);
                var query = messages.CreateQuery().Where(item => item.CreatedAt < oneWeek);
                await messages.PurgeAsync("syncPurgeOldData", query, force: true, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                await messages.PurgeAsync(force: true);
                await PullChangesAsync(true).ConfigureAwait(false);
            }
        }

        async Task InitializeOfflineStorageAsync()
        {
            if (!client.SyncContext.IsInitialized)
            {
                // Define the database schema
                var store = new MobileServiceSQLiteStore("offlinecache.db");
                store.DefineTable<CircleMessage>();

                // Create the DB file
                await client.SyncContext.InitializeAsync(store).ConfigureAwait(false);
            }
        }

        public async Task AddAsync(CircleMessage message)
        {
            Debug.Assert(message.Id == null);

            await InitializeTableAsync();
            await messages.InsertAsync(message);
            await PushChangesAsync();
        }

        public async Task<IEnumerable<CircleMessage>> GetRootsAsync()
        {
            await InitializeTableAsync();
            await PullChangesAsync();

            return await messages.Where(cm => cm.IsRoot)
                .OrderByDescending(cm => cm.CreatedAt)
                .ToEnumerableAsync();
        }

        public async Task<long> GetDetailCountAsync(string id)
        {
            Debug.Assert(messages != null);
            var result = await messages
                           .Where(cm => cm.ThreadId == id && !cm.IsRoot)
                           .IncludeTotalCount().ToEnumerableAsync()
                           .ConfigureAwait(false) as IQueryResultEnumerable<CircleMessage>;
            return result.TotalCount;
        }

        public async Task<IEnumerable<CircleMessage>> GetDetailsAsync(string id)
        {
            Debug.Assert(messages != null);
            await PullChangesAsync();

            return await messages.Where(cm => cm.ThreadId == id)
                .OrderBy(cm => cm.CreatedAt)
                .ToEnumerableAsync();
        }

        async Task PushChangesAsync()
        {
            if (!(await CrossConnectivity.Current.IsRemoteReachable(client.MobileAppUri, TimeSpan.FromSeconds(5))))
            {
                Debug.WriteLine($"Cannot connect to {client.MobileAppUri}. Appears to be offline");
                return;
            }

            try
            {
                // Push queued changes back to Azure
                await client.SyncContext.PushAsync().ConfigureAwait(false);

            }
            catch (MobileServicePushFailedException ex)
            {
                if (ex.PushResult != null)
                {
                    foreach (var error in ex.PushResult.Errors)
                    {
                        await ResolveConflictAsync(error);
                    }
                }
            }
        }

        async Task PullChangesAsync(bool forceFullSync = false)
        {
            if (!(await CrossConnectivity.Current.IsRemoteReachable(client.MobileAppUri, TimeSpan.FromSeconds(5))))
            {
                Debug.WriteLine($"Cannot connect to {client.MobileAppUri}. Appears to be offline");
                return;
            }

            // Pull changes from Azure back down to our copy
            // We pull the entire table down incrementally, unless a full sync is required.
            // We could also cache off specific queries - but that would require more round trips
            // to Azure which we want to avoid since we use all the data ..
            string queryName = $"sync_{nameof(CircleMessage)}";
            await messages.PullAsync(forceFullSync ? null:queryName, messages.CreateQuery()).ConfigureAwait(false);
        }

        async Task ResolveConflictAsync(MobileServiceTableOperationError error)
        {
            var serverItem = error.Result.ToObject<CircleMessage>();
            var localItem = error.Item.ToObject<CircleMessage>();

            if (serverItem.Equals(localItem))
            {
                // Items are identical, ignore the conflict; server wins.
                await error.CancelAndDiscardItemAsync().ConfigureAwait(false);
            }
            else
            {
                // otherwise, the client wins.
                localItem.Version = serverItem.Version;
                await error.UpdateOperationAsync(JObject.FromObject(localItem)).ConfigureAwait(false);
            }
        }
    }
}
