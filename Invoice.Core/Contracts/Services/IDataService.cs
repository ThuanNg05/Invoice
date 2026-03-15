using Invoice.Core.Models;

namespace Invoice.Core.Contracts.Services;

public interface IDataService
{
    bool IsCacheValid(string entityName);
    void InvalidateCache(string entityName);
    void InvalidateAllCaches();



    Task<IEnumerable<Customers>> GetCustomers(bool forceRefresh = false);
    Task AddCustomer(Customers customer);
    Task DeleteCustomer(long customerId);
    Task UpdateCustomer(Customers customer);



    Task<IEnumerable<Materials>> GetMaterials(bool forceRefresh = false);
    Task AddMaterial(Materials material);
    Task DeleteMaterial(string materialId);
    Task UpdateMaterial(Materials material);



    Task<IEnumerable<Frames>> GetFrames(bool forceRefresh = false);
    Task AddFrame(Frames plank);
    Task DeleteFrame(int? plankId);
    Task UpdateFrame(Frames plank);



    Task<IEnumerable<DetailPlanks>> GetPlanks(bool forceRefresh = false);
    Task AddPlank(DetailPlanks plank);
    Task DeletePlank(string plankId);
    Task UpdatePlank(DetailPlanks plank);



    Task<IEnumerable<DetailPrice>> GetPrice();
    Task UpdatePrice(DetailPrice prices);



    Task<IEnumerable<Invoices>> GetAllInvoices();
    Task<IEnumerable<History>> GetInvoiceHistory(DateTime? fromDate, DateTime? toDate, long? customerID);
    Task<IEnumerable<ProductSummary>> GetProducts(int skip, int take, string query);
    Task<Products?> GetProductById(string productId);
    Task AddProduct(Products product);
    Task UpdateProduct(Products product);
    Task DeleteProduct(string productId);
    Task<IEnumerable<Products>> GetAllProducts();
    Task SubscribeToProductsRealtime(Action<string, Products> onDataChanged);



    Task<IEnumerable<InvoiceDetail>> GetInvoiceDetails(string invoiceID);
    Task<int> GetInvoiceCountByDate(DateTime date);
    Task AddInvoice(Invoices invoice, IEnumerable<InvoiceDetail> details, IEnumerable<WarehouseTransaction> transactions);
    Task DeleteInvoiceAndRevertInventory(string invoiceId);
    Task UpdateProductInventory(string productId, int amountChange);



    Task<IEnumerable<WarehouseTransaction>> GetWarehouseTransactions();
    Task AddWarehouseTransaction(WarehouseTransaction transaction);

}