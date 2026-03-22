using Invoice.Core.Contracts.Services;
using Newtonsoft.Json;

namespace Invoice.Services;

public class ReportData
{
    public double TotalRevenue { get; set; }
    public double TotalProfit { get; set; }
    public List<MonthlyStat> MonthlyStats { get; set; } = new();
    public List<ProductStat> TopProducts { get; set; } = new();
}

public class MonthlyStat
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string Label => $"{Month}/{Year}";
    public int OrderCount { get; set; }
    public double Revenue { get; set; }
}

public class ProductStat
{
    public string ProductName { get; set; }
    public int TotalQuantity { get; set; }
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
        try
        {
            var json = await _dataService.GetDashboardData(year);
            var report = JsonConvert.DeserializeObject<ReportData>(json) ?? new ReportData();

            // Null safety for lists after deserialization
            report.MonthlyStats ??= new List<MonthlyStat>();
            report.TopProducts ??= new List<ProductStat>();

            // Ensure all 12 months are present for the chart
            for (int i = 1; i <= 12; i++)
            {
                if (!report.MonthlyStats.Any(m => m.Month == i))
                {
                    report.MonthlyStats.Add(new MonthlyStat { Month = i, Year = year, OrderCount = 0, Revenue = 0 });
                }
            }
            report.MonthlyStats = report.MonthlyStats.OrderBy(m => m.Month).ToList();

            return report;
        }
        catch (Exception)
        {
            // Fallback or empty report on error
            return new ReportData();
        }
    }
}
