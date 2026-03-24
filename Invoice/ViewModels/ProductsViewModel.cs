using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.Services;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.Services;

namespace Invoice.ViewModels;

public partial class ProductsViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;
    private const int PageSize = 20;
    private int _currentSkip = 0;
    private bool _hasMoreItems = true;
    private string _currentQuery = string.Empty;

    public ObservableCollection<ProductSummary> Source { get; } = new ObservableCollection<ProductSummary>();

    [ObservableProperty]
    private Products? _selectedProductFull;

    public ProductsViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
        WeakReferenceMessenger.Default.Register<ProductsChangedMessage>(this, (r, m) => HandleDataChange(m));
        
        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.PRODUCTS || m.EntityName == InMemoryCache.PLANKS)
            {
                if (App.MainWindow?.DispatcherQueue != null)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await ReloadFirstPage();
                    });
                }
            }
        });
    }

    public async Task ReloadFirstPage()
    {
        await ExecuteAsync(async () =>
        {
            _currentSkip = 0;
            _hasMoreItems = true;
            Source.Clear();
            await InternalLoadMoreDataAsync();
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task LoadMoreDataAsync()
    {
        if (!_hasMoreItems || IsBusy) return;

        await ExecuteAsync(async () =>
        {
            await InternalLoadMoreDataAsync();
        }, "LOAD_FAILED".GetLocalized());
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

    public async Task LoadProductForEditingAsync(long productId)
    {
        await ExecuteAsync(async () =>
        {
            SelectedProductFull = await _dataService.GetProductById(productId);
        }, "LOAD_FAILED".GetLocalized());
    }

    public async void OnNavigatedTo(object parameter)
    {
        await ReloadFirstPage();
    }

    public void OnNavigatedFrom()
    {
    }

    public async void Search(string query)
    {
        _currentQuery = query ?? string.Empty;
        await ReloadFirstPage();
    }

    public async Task AddProductAsync(Products p)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.AddProduct(p);
            await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task UpdateProductAsync(Products p)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.UpdateProduct(p);
            WeakReferenceMessenger.Default.Send(new ProductsChangedMessage(DataAction.Update, p));
            await DialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }, "FAILED_UPDATE".GetLocalized());
    }

    public async Task DeleteProductAsync(long id)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeleteProduct(id);
            WeakReferenceMessenger.Default.Send(new ProductsChangedMessage(DataAction.Delete, id));
            await DialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
        }, "FAILED_DELETE".GetLocalized());
    }

    private void HandleDataChange(ProductsChangedMessage message)
    {
        App.MainWindow?.DispatcherQueue?.TryEnqueue(() =>
        {
            switch (message.Action)
            {
                case DataAction.Update:
                    UpdateInSource(message.Product);
                    break;
                case DataAction.Delete:
                    RemoveFromSource(message.ProductId);
                    break;
            }
        });
    }    

    private void UpdateInSource(Products? product)
    {
        if (product == null) return;

        var itemToUpdate = Source.FirstOrDefault(x => x.ProductID == product.ProductID);

        if (itemToUpdate != null)
        {
            int index = Source.IndexOf(itemToUpdate);
            if (index != -1)
            {
                Source.RemoveAt(index);
                Source.Insert(index, new ProductSummary
                {
                    ProductID = product.ProductID,
                    Name = product.Name,
                    BasePrice = product.BasePrice,
                    PriceOdd = product.PriceOdd,
                    PriceEven = product.PriceEven,
                    Inventory = product.Inventory
                });
            }
        }
    }

    private void RemoveFromSource(long productId)
    {
        var itemToDelete = Source.FirstOrDefault(x => x.ProductID == productId);
        if (itemToDelete != null)
        {
            Source.Remove(itemToDelete);
        }
    }
}
