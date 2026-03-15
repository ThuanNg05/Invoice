using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class EditingInvoiceViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    public ObservableCollection<String> InvoiceCodes { get; } = new();
    public ObservableCollection<InvoiceDetail> InvoiceDetails { get; } = new();

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _createdDate = string.Empty;

    [ObservableProperty]
    private string? _selectedInvoiceID;

    public Action<string>? OnInvoiceConfirmed
    {
        get; set;
    }

    public EditingInvoiceViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    partial void OnSelectedInvoiceIDChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadInvoiceDetails(value);
        }
        else
        {
            ClearData();
        }
    }

    public async Task LoadInvoiceList()
    {
        try
        {
            var allInvoices = await _dataService.GetAllInvoices();
            if (allInvoices == null) return;

            var cutoffDate = DateTime.Now.AddDays(-30);

            var recentInvoiceIds = allInvoices
                .Where(inv =>
                {
                    if (DateTime.TryParse(inv.CreatedDate, out DateTime createdDate))
                    {
                        return createdDate >= cutoffDate;
                    }
                    return false;
                })
                .OrderByDescending(inv => inv.CreatedDate)
                .Select(inv => inv.InvoiceID);

            InvoiceCodes.Clear();
            foreach (var id in recentInvoiceIds)
            {
                InvoiceCodes.Add(id);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi load mã hoá đơn: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể tải danh sách mã hoá đơn.");
        }
    }

    private async void LoadInvoiceDetails(string id)
    {
        try
        {
            var details = await _dataService.GetInvoiceDetails(id);
            ClearData();

            if (details != null && details.Any())
            {
                foreach (var detail in details)
                {
                    InvoiceDetails.Add(detail);
                }
                var firstItem = details.First();
                CustomerName = firstItem.CustomerName;
                CreatedDate = firstItem.Invoice?.CreatedDate ?? "N/A";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi load chi tiết: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể tải chi tiết hoá đơn.");
        }
    }

    private void ClearData()
    {
        InvoiceDetails.Clear();
        CustomerName = string.Empty;
        CreatedDate = string.Empty;
    }

    [RelayCommand]
    private void ConfirmLoad()
    {
        if (string.IsNullOrEmpty(SelectedInvoiceID)) return;
        OnInvoiceConfirmed?.Invoke(SelectedInvoiceID);
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedInvoiceID = null;
    }

}