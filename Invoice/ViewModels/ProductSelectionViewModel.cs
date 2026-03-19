using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Microsoft.UI.Dispatching;
using Invoice.Contracts.Services;

namespace Invoice.ViewModels;

public partial class ProductSelectionViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;
    private ProductSelectionNavigationParameter? _navParam;
    private readonly DispatcherQueue _dispatcherQueue;
    private const int PageSize = 20;
    private int _currentSkip = 0;
    private bool _hasMoreItems = true;
    private string _currentQuery = string.Empty;

    public ObservableCollection<ProductSummary> Source { get; } = new ObservableCollection<ProductSummary>();

    public ProductSelectionViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
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
        await ExecuteAsync(async () =>
        {
            _currentSkip = 0;
            _hasMoreItems = true;
            Source.Clear();
            await InternalLoadMoreDataAsync();
        }, "Lỗi khi tải danh sách sản phẩm");
    }

    public async Task LoadMoreDataAsync()
    {
        if (!_hasMoreItems || IsBusy) return;

        await ExecuteAsync(async () =>
        {
            await InternalLoadMoreDataAsync();
        }, "Lỗi khi tải thêm sản phẩm");
    }

    private async Task InternalLoadMoreDataAsync()
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
