using CommunityToolkit.Mvvm.ComponentModel;

using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class DetailPriceViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DetailPrice? _currentDetailPrice;

    public DetailPriceViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        IsLoading = true;
        try
        {
            var prices = await _dataService.GetPrice();
            CurrentDetailPrice = prices.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load Detail Price data. {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể tải dữ liệu giá");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task UpdateDetailPriceAsync(DetailPrice price)
    {
        IsLoading = true;
        try
        {
            await _dataService.UpdatePrice(price);
            await App.ShowMessageAsync("Thông báo", "Cập nhật giá thành công.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update failed: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", $"{ex.Message}");
            return;
        }
        finally { IsLoading = false; }
    }
}