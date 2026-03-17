using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Services;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;

namespace Invoice.ViewModels;

public partial class ReportingViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;
    private readonly ReportingService _reportingService;
    private readonly ReportPdfService _pdfService;
    private readonly EmailService _emailService;

    [ObservableProperty] private bool _isLocked = true;
    [ObservableProperty] private string _passwordInput;
    [ObservableProperty] private string _errorMessage;
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private double _totalRevenue;
    [ObservableProperty] private double _totalProfit;

    public ISeries[] Series
    {
        get; set;
    }

    public ICartesianAxis[] XAxes
    {
        get; set;
    }

    public ObservableCollection<ProductStat> TopProducts { get; } = new();

    public ReportingViewModel(ReportingService reportingService, ReportPdfService pdfService, EmailService emailService, IConfiguration configuration)
    {
        _reportingService = reportingService;
        _pdfService = pdfService;
        _emailService = emailService;
        _configuration = configuration;
    }

    [RelayCommand]
    public void Unlock()
    {
        var pass = _configuration["PasswordReport:Password"];
        if (string.Equals(PasswordInput, pass, StringComparison.Ordinal))
        {
            IsLocked = false;
            ErrorMessage = "";
            LoadDataAsync();
        }
        else
        {
            ErrorMessage = "Mật khẩu không đúng!";
            PasswordInput = "";
        }
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
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
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ExportAndEmail()
    {
        IsLoading = true;
        try
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

            await App.ShowMessageAsync("Thành công", "Đã gửi báo cáo qua Email!");
        }
        catch (Exception ex)
        {
            await App.ShowMessageAsync("Lỗi", ex.Message);
        }
        finally { IsLoading = false; }
    }

    public async void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}