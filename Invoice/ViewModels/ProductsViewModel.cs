using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class ProductsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;
    private const int PageSize = 20;
    private int _currentSkip = 0;
    private bool _hasMoreItems = true;
    private string _currentQuery = string.Empty;

    public ObservableCollection<ProductSummary> Source { get; } = new ObservableCollection<ProductSummary>();

    [ObservableProperty]
    private Products _selectedProductFull;

    [ObservableProperty]
    private bool isLoading;

    public ProductsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        WeakReferenceMessenger.Default.Register<ProductsChangedMessage>(this, (r, m) => HandleDataChange(m));
        WeakReferenceMessenger.Default.Register<InventoryChangedMessage>(this, (r, m) =>
        {
            if (App.MainWindow != null && App.MainWindow.DispatcherQueue != null)
            {
                try
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            await ReloadFirstPage();
                        }
                        catch { }
                    });
                }
                catch
                {
                }
            }
        });
    }

    public async Task ReloadFirstPage()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            _currentSkip = 0;
            _hasMoreItems = true;
            Source.Clear();
            await LoadMoreDataAsync(forceLoad: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadMoreDataAsync(bool forceLoad = false)
    {
        if (!_hasMoreItems || (IsLoading && !forceLoad)) return;

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
            Debug.WriteLine($"Load Products Failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadProductForEditingAsync(long productId)
    {
        IsLoading = true;
        try
        {
            SelectedProductFull = await _dataService.GetProductById(productId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Load Detail Failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
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
        await _dataService.AddProduct(p);
    }
    public async Task UpdateProductAsync(Products p)
    {
        await _dataService.UpdateProduct(p);
        WeakReferenceMessenger.Default.Send(new ProductsChangedMessage(DataAction.Update, p));
    }
    public async Task DeleteProductAsync(long id)
    {
        await _dataService.DeleteProduct(id);
        WeakReferenceMessenger.Default.Send(new ProductsChangedMessage(DataAction.Delete, id));
    }

    private void HandleDataChange(ProductsChangedMessage message)
    {        
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
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
    private void AddToSource(Products product)
    {
        if (product == null) return;

        var summary = new ProductSummary
        {
            ProductID = product.ProductID,
            Name = product.Name,
            BasePrice = product.BasePrice,
            PriceOdd = product.PriceOdd,
            PriceEven = product.PriceEven,
            Inventory = product.Inventory
        };

        Source.Insert(0, summary);
    }
    private void UpdateInSource(Products product)
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