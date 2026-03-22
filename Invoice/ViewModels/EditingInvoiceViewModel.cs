using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class EditingInvoiceViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    public ObservableCollection<string> InvoiceCodes { get; } = new();
    public ObservableCollection<InvoiceDetail> InvoiceDetails { get; } = new();

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _createdDate = string.Empty;

    [ObservableProperty]
    private string? _selectedInvoiceID;

    public Action<string>? OnInvoiceConfirmed { get; set; }

    public EditingInvoiceViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
    }

    partial void OnSelectedInvoiceIDChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadInvoiceDetailsAsync(value);
        }
        else
        {
            ClearData();
        }
    }

    public async Task LoadInvoiceListAsync()
    {
        await ExecuteAsync(async () =>
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
        }, "LOAD_FAILED".GetLocalized());
    }

    private async Task LoadInvoiceDetailsAsync(string id)
    {
        await ExecuteAsync(async () =>
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
        }, "LOAD_FAILED".GetLocalized());
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
