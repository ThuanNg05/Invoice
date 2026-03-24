using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Polly;
using Polly.Retry;
using Supabase.Realtime.PostgresChanges;
using SupabaseClient = Supabase.Client;

namespace Invoice.Core.Services;

public partial class SupabaseDataService : IDataService
{
    private readonly SupabaseClient _client;
    private readonly InMemoryCache _cache = new();
    private readonly AsyncRetryPolicy _retryPolicy;

    public bool IsCacheValid(string entityName) => _cache.IsValid(entityName);
    public void InvalidateCache(string entityName) => _cache.Invalidate(entityName);
    public void InvalidateAllCaches() => _cache.Clear();

    public IEnumerable<Customers> CachedCustomers { get; private set; } = [];
    public IEnumerable<Materials> CachedMaterials { get; private set; } = [];
    public IEnumerable<Frames> CachedFrames { get; private set; } = [];
    public IEnumerable<DetailPlanks> CachedPlanks { get; private set; } = [];
    public IEnumerable<DetailPrice> CachedPrices { get; private set; } = [];

    public SupabaseDataService(SupabaseClient client)
    {
        _client = client;
        
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<Exception>(ex => ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Debug.WriteLine($"[POLLY RETRY {retryCount}] Lỗi: {exception.Message}. Thử lại sau {timeSpan.TotalSeconds}s...");
                });

        _ = SetupRealtimeCacheInvalidation();
    }

    private async Task SetupRealtimeCacheInvalidation()
    {
        try
        {
            await EnsureConnectionAsync();
            await _client.Realtime.ConnectAsync();

            await _client.From<Customers>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.CUSTOMERS));
            await _client.From<Materials>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.MATERIALS));
            await _client.From<Frames>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.FRAMES));
            await _client.From<DetailPlanks>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.PLANKS));
            await _client.From<DetailPrice>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.PRICES));
            await _client.From<Products>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.PRODUCTS));
            await _client.From<Invoices>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.INVOICES));
            await _client.From<WarehouseTransaction>().On(PostgresChangesOptions.ListenType.All, (s, c) => InvalidateAndBroadcast(InMemoryCache.TRANSACTIONS));
            
            Debug.WriteLine("[REALTIME] Cache Invalidation and Refresh system is active.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[REALTIME CACHE ERROR] {ex.Message}");
        }
    }

    private void InvalidateAndBroadcast(string key)
    {
        _cache.Invalidate(key);
        WeakReferenceMessenger.Default.Send(new DatabaseChangedMessage(key));
    }

    private async Task EnsureConnectionAsync()
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            if (_client.Auth.CurrentSession == null)
            {
                await _client.InitializeAsync();
            }
        });
    }
}
