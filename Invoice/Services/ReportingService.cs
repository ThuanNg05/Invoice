using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.Services;

public class ReportData
{
    public double TotalRevenue
    {
        get; set;
    }
    public double TotalProfit
    {
        get; set;
    }
    public List<MonthlyStat> MonthlyStats { get; set; } = new();
    public List<ProductStat> TopProducts { get; set; } = new();
}

public class MonthlyStat
{
    public int Month
    {
        get; set;
    }
    public int Year
    {
        get; set;
    }
    public string Label => $"{Month}/{Year}";
    public int OrderCount
    {
        get; set;
    }
    public double Revenue
    {
        get; set;
    }
}

public class ProductStat
{
    public string ProductName
    {
        get; set;
    }
    public int TotalQuantity
    {
        get; set;
    }
}

public class ReportingService
{
    private readonly IDataService _dataService;

    public ReportingService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<ReportData> GetDashboardDataAsync(int year)
    {
        // 1. Fetch all necessary data
        // Note: For large datasets, it is better to do aggregation in Supabase (SQL). 
        // For local apps, fetching all is acceptable for < 10,000 records.
        var invoices = await _dataService.GetAllInvoices();
        var allProducts = await _dataService.GetAllProducts();

        // We need details for all invoices to calculate profit
        var allDetails = new List<InvoiceDetail>();
        foreach (var inv in invoices)
        {
            var details = await _dataService.GetInvoiceDetails(inv.InvoiceID);
            allDetails.AddRange(details);
        }

        // 2. Filter by Year
        //var currentYearInvoices = invoices
        //    .Where(i => DateTime.Parse(i.CreatedDate).Year == year)
        //    .ToList();

        var currentYearInvoices = invoices.Where(i =>
        {            
            if (DateTime.TryParse(i.CreatedDate, out DateTime date))
            {
                return date.Year == year;
            }
            return false;
        }).ToList();

        var currentYearDetails = allDetails
            .Where(d => currentYearInvoices.Any(i => i.InvoiceID == d.InvoiceID))
            .ToList();

        // 3. Calculate Stats
        var report = new ReportData();
        report.TotalRevenue = currentYearInvoices.Sum(i => i.Total);

        // Profit Calculation: Sum((SellPrice - BasePrice) * Amount)
        // We create a dictionary for fast product lookup
        var productMap = allProducts.ToDictionary(p => p.ProductID, p => p.BasePrice);

        report.TotalProfit = currentYearDetails.Sum(d =>
        {
            double cost = productMap.ContainsKey(d.ProductID) ? productMap[d.ProductID] : 0;
            return (d.SellPrice - cost) * d.Amount;
        });

        // 4. Monthly Stats (Bar Chart Data)
        var groupedByMonth = currentYearInvoices
            .GroupBy(i => DateTime.Parse(i.CreatedDate).Month)
            .OrderBy(g => g.Key);

        foreach (var group in groupedByMonth)
        {
            report.MonthlyStats.Add(new MonthlyStat
            {
                Month = group.Key,
                Year = year,
                OrderCount = group.Count(),
                Revenue = group.Sum(x => x.Total)
            });
        }

        // Fill missing months with 0
        for (int i = 1; i <= 12; i++)
        {
            if (!report.MonthlyStats.Any(m => m.Month == i))
                report.MonthlyStats.Add(new MonthlyStat { Month = i, Year = year, OrderCount = 0, Revenue = 0 });
        }
        report.MonthlyStats = report.MonthlyStats.OrderBy(m => m.Month).ToList();

        // 5. Top 10 Products
        report.TopProducts = currentYearDetails
            .GroupBy(d => d.ProductName)
            .Select(g => new ProductStat
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(10)
            .ToList();

        return report;
    }
}