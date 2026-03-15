using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class HistoryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    private List<Customers> _allCustomers = new();
    public ObservableCollection<Customers> CustomersList { get; } = new ObservableCollection<Customers>();
    public ObservableCollection<History> Source { get; } = new ObservableCollection<History>();

    [ObservableProperty]
    private Customers? _selectedCustomer;

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    public HistoryViewModel(IDataService dataService)
    {
        _dataService = dataService;
        var now = DateTime.Now;
        StartDate = null;
        EndDate = null;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadCustomerList();
        await ResetQuery();
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadCustomerList()
    {
        IsLoading = true;
        try
        {
            var customers = await _dataService.GetCustomers();
            _allCustomers = customers.ToList();
            CustomersList.Clear();
            foreach (var customer in _allCustomers)
            {
                CustomersList.Add(customer);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load customer failed: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Lỗi load tên khách hàng.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchInvoice()
    {
        Source.Clear();
        IsLoading = true;

        DateTime? from = StartDate?.DateTime;
        DateTime? to = EndDate?.DateTime;
        long? idToFind = SelectedCustomer?.CustomerID;

        var historyData = await _dataService.GetInvoiceHistory(from, to, idToFind);

        foreach (var item in historyData)
        {
            Source.Add(item);
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task ResetQuery()
    {
        SelectedCustomer = null;
        var now = DateTime.Now;
        StartDate = null;
        EndDate = null;
        Source.Clear();
    }
}