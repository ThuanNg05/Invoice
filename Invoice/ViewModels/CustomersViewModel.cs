using System.Collections.ObjectModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class CustomersViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public List<Customers> AllCustomers = new();
    public ObservableCollection<Customers> CustomersCollection { get; } = new();

    public CustomersViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
    }

    public void OnNavigatedTo(object parameter)
    {
        _ = LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            CustomersCollection.Clear();
            var data = await _dataService.GetCustomers(forceRefresh: false);
            AllCustomers = data.ToList();
            foreach (var item in AllCustomers)
            {
                CustomersCollection.Add(item);
            }
        }, "Lỗi khi tải danh sách khách hàng");
    }

    public async Task AddCustomerAsync(Customers customer)
    {
        if (CustomersCollection.Any(c => c.Name.Equals(customer.Name, StringComparison.OrdinalIgnoreCase)))
        {
            await DialogService.ShowErrorAsync("Tên khách hàng đã tồn tại!");
            return;
        }
        if (!string.IsNullOrEmpty(customer.Phone) &&
            CustomersCollection.Any(c => c.Phone == customer.Phone))
        {
            await DialogService.ShowErrorAsync("Số điện thoại đã tồn tại!");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddCustomer(customer);
            CustomersCollection.Add(customer);
            AllCustomers.Add(customer);
            await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
        }, "Lỗi khi thêm khách hàng");
    }

    public async Task DeleteCustomerAsync(Customers customers)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeleteCustomer(customers.CustomerID);
            CustomersCollection.Remove(customers);
            var itemInAll = AllCustomers.FirstOrDefault(c => c.CustomerID == customers.CustomerID);
            if (itemInAll != null) AllCustomers.Remove(itemInAll);
            await DialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
        }, "Lỗi khi xóa khách hàng");
    }

    public async Task UpdateCustomerAsync(Customers customer)
    {
        if (!string.IsNullOrEmpty(customer.Phone) &&
            CustomersCollection.Any(c => c.Phone == customer.Phone && c.CustomerID != customer.CustomerID))
        {
            await DialogService.ShowErrorAsync("Số điện thoại đã tồn tại!");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.UpdateCustomer(customer);

            var itemToUpdate = CustomersCollection.FirstOrDefault(c => c.CustomerID == customer.CustomerID);
            if (itemToUpdate != null)
            {
                var index = CustomersCollection.IndexOf(itemToUpdate);
                if (index != -1)
                {
                    CustomersCollection.RemoveAt(index);
                    CustomersCollection.Insert(index, customer);
                }
            }

            var itemInAll = AllCustomers.FirstOrDefault(c => c.CustomerID == customer.CustomerID);
            if (itemInAll != null)
            {
                var indexAll = AllCustomers.IndexOf(itemInAll);
                if (indexAll != -1)
                {
                    AllCustomers[indexAll] = customer;
                }
            }
            await DialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }, "Lỗi khi cập nhật khách hàng");
    }
}
