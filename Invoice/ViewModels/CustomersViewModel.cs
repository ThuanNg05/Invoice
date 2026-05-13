using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
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

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.CUSTOMERS)
            {
                if (App.MainWindow?.DispatcherQueue != null)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await LoadDataAsync();
                    });
                }
            }
        });
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
        customer.Name = StringHelper.NormalizeVietnameseName(customer.Name);
        // 1. Check by Name
        var existingByName = await _dataService.GetCustomerByName(customer.Name);
        if (existingByName != null)
        {
            var result = await DialogService.ShowConfirmAsync("Thông báo", $"Khách hàng '{customer.Name}' đã tồn tại trong hệ thống. Bạn có muốn phục hồi và thay thế dữ liệu mới không?");
            if (result)
            {
                // Check if the NEW phone conflicts with ANOTHER customer
                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    var existingByPhone = await _dataService.GetCustomerByPhone(customer.Phone);
                    if (existingByPhone != null && existingByPhone.CustomerID != existingByName.CustomerID)
                    {
                        await DialogService.ShowErrorAsync($"Số điện thoại '{customer.Phone}' đã được sử dụng bởi khách hàng '{existingByPhone.Name}'!");
                        return;
                    }
                }

                await ExecuteAsync(async () =>
                {
                    await _dataService.HardDeleteCustomer(existingByName.CustomerID);
                    await _dataService.AddCustomer(customer);
                    
                    var currentInList = CustomersCollection.FirstOrDefault(c => c.Name.Equals(customer.Name, StringComparison.OrdinalIgnoreCase));
                    if (currentInList != null)
                    {
                        CustomersCollection.Remove(currentInList);
                        AllCustomers.RemoveAll(c => c.Name.Equals(customer.Name, StringComparison.OrdinalIgnoreCase));
                    }

                    CustomersCollection.Add(customer);
                    AllCustomers.Add(customer);
                    await DialogService.ShowSuccessAsync("Thêm thành công");
                }, "Lỗi khi phục hồi khách hàng");
            }
            return;
        }

        // 2. Check by Phone
        if (!string.IsNullOrEmpty(customer.Phone))
        {
            var existingByPhone = await _dataService.GetCustomerByPhone(customer.Phone);
            if (existingByPhone != null)
            {
                var result = await DialogService.ShowConfirmAsync("Thông báo", $"Số điện thoại '{customer.Phone}' đã tồn tại (Khách hàng: {existingByPhone.Name}). Bạn có muốn phục hồi khách hàng này và cập nhật thông tin mới không?");
                if (result)
                {
                    await ExecuteAsync(async () =>
                    {
                        await _dataService.HardDeleteCustomer(existingByPhone.CustomerID);
                        await _dataService.AddCustomer(customer);
                        
                        var currentInList = CustomersCollection.FirstOrDefault(c => c.Phone == customer.Phone);
                        if (currentInList != null)
                        {
                            CustomersCollection.Remove(currentInList);
                            AllCustomers.RemoveAll(c => c.Phone == customer.Phone);
                        }

                        CustomersCollection.Add(customer);
                        AllCustomers.Add(customer);
                        await DialogService.ShowSuccessAsync("Thêm thành công");
                    }, "Lỗi khi phục hồi khách hàng");
                }
                return;
            }
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddCustomer(customer);
            CustomersCollection.Add(customer);
            AllCustomers.Add(customer);
            await DialogService.ShowSuccessAsync("Thêm thành công");
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
            await DialogService.ShowSuccessAsync("Xóa thành công");
        }, "Lỗi khi xóa khách hàng");
    }

    public async Task UpdateCustomerAsync(Customers customer)
    {
        customer.Name = StringHelper.NormalizeVietnameseName(customer.Name);
        if (!string.IsNullOrEmpty(customer.Phone))
        {
            var existingByPhone = await _dataService.GetCustomerByPhone(customer.Phone);
            if (existingByPhone != null && existingByPhone.CustomerID != customer.CustomerID)
            {
                await DialogService.ShowErrorAsync($"Số điện thoại '{customer.Phone}' đã tồn tại (Khách hàng: {existingByPhone.Name})!");
                return;
            }
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
            await DialogService.ShowSuccessAsync("Cập nhật thành công");
        }, "Lỗi khi cập nhật khách hàng");
    }
}
