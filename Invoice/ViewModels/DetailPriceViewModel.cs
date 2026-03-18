using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
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
            Debug.WriteLine($"Failed to load Detail Price data. {ex.Message}");            
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update failed: {ex.Message}");            
            return;
        }
        finally { IsLoading = false; }
    }
}