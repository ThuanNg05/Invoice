using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Contracts.Services;
using Invoice.Services;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class ReportingViewModel : ViewModelBase
{
    private readonly IConfiguration _configuration;
    private readonly ReportingService _reportingService;
    private readonly ReportPdfService _pdfService;
    private readonly EmailService _emailService;

    [ObservableProperty] private bool _isLocked = true;
    [ObservableProperty] private string _passwordInput = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private double _totalRevenue;
    [ObservableProperty] private double _totalProfit;    

    [ObservableProperty] private ISeries[]? _series;
    [ObservableProperty] private ICartesianAxis[]? _xAxes;

    public ObservableCollection<ProductStat> TopProducts { get; } = new();

    public ReportingViewModel(
        ReportingService reportingService, 
        ReportPdfService pdfService, 
        EmailService emailService, 
        IConfiguration configuration,
        IDialogService dialogService) : base(dialogService)
    {
        _reportingService = reportingService;
        _pdfService = pdfService;
        _emailService = emailService;
        _configuration = configuration;
    }

    [RelayCommand]
    public async Task Unlock()
    {
        try 
        {
            var pass = _configuration["PasswordReport:Password"];
            if (string.Equals(PasswordInput, pass, StringComparison.Ordinal))
            {
                await LoadDataAsync();
                IsLocked = false;
                ErrorMessage = "";
            }
            else
            {
                ErrorMessage = "Mật khẩu không đúng!";
                PasswordInput = "";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unlock Error: {ex.Message}");
            ErrorMessage = "Lỗi khi mở khóa dữ liệu.";
        }
    }

    public async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var data = await _reportingService.GetDashboardDataAsync(DateTime.Now.Year);

            TotalRevenue = data.TotalRevenue;
            TotalProfit = data.TotalProfit;

            TopProducts.Clear();
            if (data.TopProducts != null)
            {
                foreach (var p in data.TopProducts) TopProducts.Add(p);
            }

            // Setup Chart - Minimal for stability test
            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Số đơn hàng",
                    Values = data.MonthlyStats.Select(x => (double)x.OrderCount).ToArray()
                }
            };

            XAxes = new ICartesianAxis[]
            {
                new Axis
                {
                    Labels = data.MonthlyStats.Select(x => x.Label).ToArray(),
                    LabelsRotation = 15
                }
            };
        }, "LOAD_FAILED".GetLocalized());
    }

    [RelayCommand]
    public async Task ExportAndEmail()
    {
        await ExecuteAsync(async () =>
        {
            var data = await _reportingService.GetDashboardDataAsync(DateTime.Now.Year);
            string path = _pdfService.GenerateFinancialReport(data, DateTime.Now.Year);
            var receiverEmail = _configuration["EmailSettings:ReceiverEmail"];

            // Send Email
            await _emailService.SendReportEmailAsync(
                receiverEmail,
                $"Báo cáo tài chính {DateTime.Now.Year}",
                "Gửi kèm báo cáo tài chính.",
                path
            );

            await DialogService.ShowSuccessAsync("Thông báo");
        }, "Reporting_Error_Export");
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
