using Invoice.Core.Models;

namespace Invoice.Core.Contracts.Services;

public interface ICustomerService
{
    Task<IEnumerable<Customers>> GetCustomers(bool forceRefresh = false);
    Task AddCustomer(Customers customer);
    Task DeleteCustomer(long customerId);
    Task UpdateCustomer(Customers customer);
}

public interface IProductService
{
    Task<IEnumerable<ProductSummary>> GetProducts(int skip, int take, string query);
    Task<Products?> GetProductById(long productId);
    Task AddProduct(Products product);
    Task UpdateProduct(Products product);
    Task DeleteProduct(long productId);
    Task<IEnumerable<Products>> GetAllProducts();
    Task SubscribeToProductsRealtime(Action<string, Products> onDataChanged);
    
    Task<IEnumerable<Materials>> GetMaterials(bool forceRefresh = false);
    Task AddMaterial(Materials material);
    Task DeleteMaterial(long productId);
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
}

public interface IInvoiceService
{
    Task<IEnumerable<Invoices>> GetAllInvoices();
    Task<IEnumerable<History>> GetInvoiceHistory(DateTime? fromDate, DateTime? toDate, long? customerID);
    Task<IEnumerable<InvoiceDetail>> GetInvoiceDetails(string invoiceID);
    Task<int> GetInvoiceCountByDate(DateTime date);
    Task AddInvoice(Invoices invoice, IEnumerable<InvoiceDetail> details, IEnumerable<WarehouseTransaction> transactions);
    Task DeleteInvoiceAndRevertInventory(string invoiceId);
    Task<string> GetDashboardData(int year);
}

public interface IInventoryService
{
    Task UpdateProductInventory(long productId, int amountChange);
    Task<IEnumerable<WarehouseTransaction>> GetWarehouseTransactions();
    Task AddWarehouseTransaction(WarehouseTransaction transaction);
    Task ProcessInventoryTransaction(Frames frame, int amount, long? sourcePlankId = null);
    Task<bool> ValidateMaterialStock(long productId, int requiredAmount);
}
