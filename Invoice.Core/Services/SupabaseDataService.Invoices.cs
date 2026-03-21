using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Supabase.Postgrest.Interfaces;
using static Supabase.Postgrest.Constants;

namespace Invoice.Core.Services;

public partial class SupabaseDataService
{
    public async Task<IEnumerable<Invoices>> GetAllInvoices()
    {
        await EnsureConnectionAsync();        
        var response = await _client.From<Invoices>().Get();        
        return response.Models;
    }

    public async Task<IEnumerable<History>> GetInvoiceHistory(DateTime? fromDate, DateTime? toDate, long? customerId)
    {
        await EnsureConnectionAsync();        
        var query = _client.From<InvoiceDetail>()
                       .Select("*, Invoice:invoices!inner(*)");        

        if (fromDate.HasValue)
        {            
            string fromStr = fromDate.Value.ToString("yyyy-MM-dd");

            if (toDate.HasValue)
            {                
                string toStr = toDate.Value.ToString("yyyy-MM-dd");
                query = query.Filter("Invoice.created_date", Operator.GreaterThanOrEqual, fromStr)
                             .Filter("Invoice.created_date", Operator.LessThanOrEqual, toStr);
            }            
            else
            {
                query = query.Filter("Invoice.created_date", Operator.Equals, fromStr);
            }
        }
        else if (toDate.HasValue)
        {
            string toStr = toDate.Value.ToString("yyyy-MM-dd");
            query = query.Filter("Invoice.created_date", Operator.LessThanOrEqual, toStr);
        }

        if (customerId.HasValue)
        {            
            query = query.Filter("Invoice.customer_id", Operator.Equals, customerId.Value.ToString());
        }

        var response = await query.Get();
        
        var result = response.Models.Select(d => new History
        {
            InvoiceID = d.InvoiceID,            
            CreatedDate = DateTime.TryParse(d.Invoice?.CreatedDate, out var date) ? date : DateTime.MinValue,
            CustomerName = d.CustomerName,
            ProductID = d.ProductID,
            ProductName = d.ProductName,
            SellPrice = d.SellPrice,
            Amount = d.Amount,
            LineTotal = d.LineTotal ?? 0,
            Note = d.Note
        });

        return [.. result.OrderByDescending(x => x.CreatedDate).ThenBy(x => x.InvoiceID)];
    }

    public async Task<IEnumerable<InvoiceDetail>> GetInvoiceDetails(string invoiceID)
    {
        await EnsureConnectionAsync();
        var response = await _client.From<InvoiceDetail>().Where(i => i.InvoiceID == invoiceID).Get();
        return response.Models;
    }

    public async Task<int> GetInvoiceCountByDate(DateTime date)
    {
        await EnsureConnectionAsync();
        string dateString = date.ToString("yyyy-MM-dd");
        var response = await _client.From<Invoices>().Where(x => x.CreatedDate == dateString)
            .Count(CountType.Exact);
        return response;
    }

    public async Task AddInvoice(Invoices invoice, IEnumerable<InvoiceDetail> details, IEnumerable<WarehouseTransaction> transactions)
    {
        await EnsureConnectionAsync();

        try
        {            
            await _client.Rpc("create_full_invoice", new
            {
                invoice_data = invoice,
                details_data = details ?? new List<InvoiceDetail>(),
                transactions_data = transactions ?? new List<WarehouseTransaction>()
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create invoice: {ex.Message}");            
        }
    }
    
    public async Task DeleteInvoiceAndRevertInventory(string invoiceId)
    {
        await EnsureConnectionAsync();

        try 
        {            
            await _client.Rpc("delete_invoice_and_revert", new
            {
                p_invoice_id = invoiceId
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi xoá hoá đơn: {ex.Message}");
        }
    }

    public async Task<string> GetDashboardData(int year)
    {
        await EnsureConnectionAsync();
        var response = await _client.Rpc("get_annual_report", new { target_year = year });
        return response.Content;
    }
}
