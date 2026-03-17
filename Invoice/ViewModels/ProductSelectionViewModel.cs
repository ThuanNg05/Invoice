using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Microsoft.UI.Dispatching;

namespace Invoice.ViewModels;

public partial class ProductSelectionViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;
    private ProductSelectionNavigationParameter _navParam;
    private readonly DispatcherQueue _dispatcherQueue;
    private const int PageSize = 20;
    private int _currentSkip = 0;
    private bool _hasMoreItems = true;
    private string _currentQuery = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<ProductSummary> Source { get; } = new ObservableCollection<ProductSummary>();

    public ProductSelectionViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async void FilterData(string query)
    {
        _currentQuery = query;
        await ReloadFirstPage();
    }

    public async Task ReloadFirstPage()
    {
        if (IsLoading) return;
        //IsLoading = true;
        try
        {
            _currentSkip = 0;
            _hasMoreItems = true;
            Source.Clear();
            await LoadMoreDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reloading: {ex.Message}");
        }
    }

    public async Task LoadMoreDataAsync()
    {
        if (!_hasMoreItems || IsLoading) return;

        IsLoading = true;
        try
        {
            var items = await _dataService.GetProducts(_currentSkip, PageSize, _currentQuery);
            var list = items.ToList();

            if (list.Count < PageSize)
            {
                _hasMoreItems = false;
            }

            foreach (var item in list)
            {
                Source.Add(item);
            }
            _currentSkip += list.Count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<Products> GetProductDetailAsync(long productId)
    {
        return await _dataService.GetProductById(productId);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is ProductSelectionNavigationParameter navParam)
        {
            _navParam = navParam;
        }
        await ReloadFirstPage();
    }

    public void OnNavigatedFrom()
    {
    }
}