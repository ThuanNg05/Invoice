using System.Diagnostics;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.Core.Services;

public partial class SupabaseDataService
{
    public async Task<IEnumerable<Customers>> GetCustomers(bool forceRefresh = false)
    {
        if (!forceRefresh && _cache.TryGet<List<Customers>>(InMemoryCache.CUSTOMERS, out var cached))
        {
            Debug.WriteLine("[CACHE HIT] Customers");
            return cached;
        }
        
        Debug.WriteLine("[CACHE MISS] Customers — fetching from server");
        await EnsureConnectionAsync();

        var response = await _client.From<Customers>().Get();
        var sorted = response.Models.OrderBy(c => c.Name).ToList();

        _cache.Set(InMemoryCache.CUSTOMERS, sorted, TimeSpan.FromMinutes(5));

        return sorted;
    }

    public async Task AddCustomer(Customers customer)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<Customers>().Insert(customer);
        var newCustomer = response.Models.FirstOrDefault();

        if (newCustomer != null)
        {
            customer.CustomerID = newCustomer.CustomerID;
        }

        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }

    public async Task DeleteCustomer(long customerId)
    {
        await EnsureConnectionAsync();
        await _client.From<Customers>().Where(c => c.CustomerID == customerId).Delete();
        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }

    public async Task UpdateCustomer(Customers customer)
    {
        await EnsureConnectionAsync();
        await _client.From<Customers>().Update(customer);
        _cache.Invalidate(InMemoryCache.CUSTOMERS);
    }
}
