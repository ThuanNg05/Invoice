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

    public ISeries[] Series { get; set; } = [];
    public ICartesianAxis[] XAxes { get; set; } = [];

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
        var pass = _configuration["PasswordReport:Password"];
        if (string.Equals(PasswordInput, pass, StringComparison.Ordinal))
        {
            IsLocked = false;
            ErrorMessage = "";
            await LoadDataAsync();
        }
        else
        {
            ErrorMessage = "Mật khẩu không đúng!"; // Internal error, usually not localized or move to resw if needed
            PasswordInput = "";
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
            foreach (var p in data.TopProducts) TopProducts.Add(p);

            // Setup Chart
            Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Đơn hàng: ",
                    Values = data.MonthlyStats.Select(x => x.OrderCount).ToArray()
                }
            };

            XAxes = new ICartesianAxis[]
            {
                new Axis
                {
                    Labels = data.MonthlyStats.Select(x => x.Label).ToList(),
                    LabelsRotation = 15
                }
            };
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
        }, "Reporting_Error_Load".GetLocalized());
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

            await DialogService.ShowSuccessAsync("Common_Success".GetLocalized());
        }, "Reporting_Error_Export".GetLocalized());
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
