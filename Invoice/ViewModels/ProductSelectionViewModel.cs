using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Microsoft.UI.Dispatching;
using Invoice.Contracts.Services;
using Invoice.Core.Contracts;

namespace Invoice.ViewModels;

public partial class ProductSelectionViewModel : ViewModelBase, INavigationAware, IRecipient<ProductsSelectedMessage>
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
        WeakReferenceMessenger.Default.Register<ProductsSelectedMessage>(this);

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.PRODUCTS || m.EntityName == InMemoryCache.PLANKS)
            {
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    await ReloadFirstPage();
                });
            }
        });
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
            item.OriginalInventory = item.Inventory;

            // Subtract pending quantities from temp invoice
            if (_navParam?.CurrentInvoiceItems != null)
            {
                var pendingAmount = _navParam.CurrentInvoiceItems
                    .Where(x => x.ProductID == item.ProductID)
                    .Sum(x => x.Amount);
                
                item.Inventory -= pendingAmount;
            }

            Source.Add(item);
        }
        _currentSkip += list.Count;
    }

    public async Task<Products> GetProductDetailAsync(long productId)
    {
        return await _dataService.GetProductById(productId);
    }

    private void SubscribeToInvoiceChanges()
    {
        if (_navParam?.CurrentInvoiceItems is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged += OnInvoiceItemsChanged;
        }

        if (_navParam?.CurrentInvoiceItems != null)
        {
            foreach (var item in _navParam.CurrentInvoiceItems)
            {
                item.PropertyChanged += OnInvoiceItemPropertyChanged;
            }
        }
    }

    private void UnsubscribeFromInvoiceChanges()
    {
        if (_navParam?.CurrentInvoiceItems is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= OnInvoiceItemsChanged;
        }

        if (_navParam?.CurrentInvoiceItems != null)
        {
            foreach (var item in _navParam.CurrentInvoiceItems)
            {
                item.PropertyChanged -= OnInvoiceItemPropertyChanged;
            }
        }
    }

    private void OnInvoiceItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TempInvoice item in e.NewItems)
            {
                item.PropertyChanged += OnInvoiceItemPropertyChanged;
                UpdateInventoryForProduct(item.ProductID);
            }
        }

        if (e.OldItems != null)
        {
            foreach (TempInvoice item in e.OldItems)
            {
                item.PropertyChanged -= OnInvoiceItemPropertyChanged;
                UpdateInventoryForProduct(item.ProductID);
            }
        }
    }

    private void OnInvoiceItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is TempInvoice item && (e.PropertyName == nameof(TempInvoice.Amount)))
        {
            UpdateInventoryForProduct(item.ProductID);
        }
    }

    private void UpdateInventoryForProduct(long productId)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var summary = Source.FirstOrDefault(x => x.ProductID == productId);
            if (summary != null && _navParam?.CurrentInvoiceItems != null)
            {
                var pendingAmount = _navParam.CurrentInvoiceItems
                    .Where(x => x.ProductID == productId)
                    .Sum(x => x.Amount);
                summary.Inventory = summary.OriginalInventory - pendingAmount;
            }
        });
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is ProductSelectionNavigationParameter navParam)
        {
            _navParam = navParam;
            SubscribeToInvoiceChanges();
        }
        await ReloadFirstPage();
    }

    public void OnNavigatedFrom()
    {
        UnsubscribeFromInvoiceChanges();
        WeakReferenceMessenger.Default.Unregister<ProductsSelectedMessage>(this);
    }

    public void Receive(ProductsSelectedMessage message)
    {
        UpdateInventoryForProduct(message.Product.ProductID);
    }
}
