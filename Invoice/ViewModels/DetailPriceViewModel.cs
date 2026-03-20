using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class DetailPriceViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private DetailPrice? _currentDetailPrice;

    public DetailPriceViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
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
            var prices = await _dataService.GetPrice();
            CurrentDetailPrice = prices.FirstOrDefault();
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task UpdateDetailPriceAsync(DetailPrice price)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.UpdatePrice(price);
        }, "FAILED_UPDATE".GetLocalized());
    }
}
