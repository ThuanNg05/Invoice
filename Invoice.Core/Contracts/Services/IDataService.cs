using Invoice.Core.Models;

namespace Invoice.Core.Contracts.Services;

public interface IDataService : ICustomerService, IProductService, IInvoiceService, IInventoryService
{
    // Caching methods
    bool IsCacheValid(string entityName);
    void InvalidateCache(string entityName);
    void InvalidateAllCaches();
    
    // Explicitly keeping some properties if they are used directly from IDataService
    IEnumerable<Customers> CachedCustomers { get; }
    IEnumerable<Materials> CachedMaterials { get; }
    IEnumerable<Frames> CachedFrames { get; }
    IEnumerable<DetailPlanks> CachedPlanks { get; }
    IEnumerable<DetailPrice> CachedPrices { get; }
}
