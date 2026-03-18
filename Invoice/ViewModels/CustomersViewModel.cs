using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class CustomersViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    public List<Customers> AllCustomers = new();
    public ObservableCollection<Customers> CustomersCollection { get; } = new();

    public CustomersViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public void OnNavigatedTo(object parameter)
    {
        _ = LoadDataSafeAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task LoadDataSafeAsync()
    {
        try { await LoadData(); }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await App.ShowMessageAsync("Lỗi", "Không thể tải dữ liệu.");
        }
    }

    private async Task LoadData()
    {
        IsLoading = true;
        CustomersCollection.Clear();
        var data = await _dataService.GetCustomers(forceRefresh: false);
        AllCustomers = data.ToList();
        foreach (var item in AllCustomers)
        {
            CustomersCollection.Add(item);
        }
        IsLoading = false;
    }

    public async Task AddCustomerAsync(Customers customer)
    {
        if (CustomersCollection.Any(c => c.Name.Equals(customer.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Debug.WriteLine("Lỗi: Tên khách hàng đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Tên khách hàng đã tồn tại!");
            return;
        }
        if (!string.IsNullOrEmpty(customer.Phone) &&
        CustomersCollection.Any(c => c.Phone == customer.Phone))
        {
            Debug.WriteLine("Lỗi: Số điện thoại đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Số điện thoại đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.AddCustomer(customer);
            CustomersCollection.Add(customer);
            AllCustomers.Add(customer);            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi thêm: {ex.Message}");            
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteCustomerAsync(Customers customers)
    {
        IsLoading = true;
        try
        {
            await _dataService.DeleteCustomer(customers.CustomerID);
            CustomersCollection.Remove(customers);
            var itemInAll = AllCustomers.FirstOrDefault(c => c.CustomerID == customers.CustomerID);
            if (itemInAll != null) AllCustomers.Remove(itemInAll);            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi xóa: {ex.Message}");            
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateCustomerAsync(Customers customer)
    {
        if (!string.IsNullOrEmpty(customer.Phone) &&
        CustomersCollection.Any(c => c.Phone == customer.Phone && c.CustomerID != customer.CustomerID))
        {
            Debug.WriteLine("Lỗi: Số điện thoại đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Số điện thoại đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to update customer: {ex.Message}");            
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }
}