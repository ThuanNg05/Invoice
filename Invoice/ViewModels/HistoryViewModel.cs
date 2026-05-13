using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class HistoryViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

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

    public HistoryViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
        StartDate = null;
        EndDate = null;

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.INVOICES || m.EntityName == InMemoryCache.CUSTOMERS)
            {
                if (App.MainWindow?.DispatcherQueue != null)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        if (m.EntityName == InMemoryCache.CUSTOMERS)
                        {
                            await LoadCustomerList();
                        }
                        await SearchInvoice();
                    });
                }
            }
        });
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
        await ExecuteAsync(async () =>
        {
            var customers = await _dataService.GetCustomers();
            _allCustomers = customers.ToList();
            CustomersList.Clear();
            foreach (var customer in _allCustomers)
            {
                CustomersList.Add(customer);
            }
        }, "Lỗi nạp dữ liệu");
    }

    [RelayCommand]
    public async Task SearchInvoice()
    {
        await ExecuteAsync(async () =>
        {
            Source.Clear();
            DateTime? from = StartDate?.DateTime;
            DateTime? to = EndDate?.DateTime;
            long? idToFind = SelectedCustomer?.CustomerID;

            var historyData = await _dataService.GetInvoiceHistory(from, to, idToFind);

            foreach (var item in historyData)
            {
                Source.Add(item);
            }
        }, "Lỗi nạp dữ liệu");
    }

    [RelayCommand]
    public async Task ResetQuery()
    {
        SelectedCustomer = null;
        StartDate = null;
        EndDate = null;
        Source.Clear();
        await Task.CompletedTask;
    }
}
