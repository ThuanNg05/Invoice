using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class IOActionsViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    private List<InventoryItem> _allItems = [];
    public ObservableCollection<InventoryItem> SourceList { get; } = [];
    public ObservableCollection<WarehouseTransaction> TransactionList { get; } = [];

    public IOActionsViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
        TransactionList.CollectionChanged += OnTransactionListChanged;

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.PRODUCTS || m.EntityName == InMemoryCache.MATERIALS || 
                m.EntityName == InMemoryCache.PLANKS || m.EntityName == InMemoryCache.TRANSACTIONS)
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

    private void OnTransactionListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (WarehouseTransaction item in e.NewItems)
                item.PropertyChanged += OnTransactionItemPropertyChanged;
        }

        if (e.OldItems != null)
        {
            foreach (WarehouseTransaction item in e.OldItems)
                item.PropertyChanged -= OnTransactionItemPropertyChanged;
        }

        UpdateTemporaryInventories();
    }

    private void OnTransactionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WarehouseTransaction.Amount) || e.PropertyName == nameof(WarehouseTransaction.ActionType))
        {
            UpdateTemporaryInventories();
        }
    }

    private void UpdateTemporaryInventories()
    {
        // Reset all to base inventory
        foreach (var item in _allItems)
        {
            item.TemporaryInventory = item.Inventory;
        }

        // Apply pending changes
        foreach (var trans in TransactionList)
        {
            var item = _allItems.FirstOrDefault(x => x.ProductID == trans.ProductID);
            if (item != null)
            {
                item.TemporaryInventory += trans.FinalChange;
            }
        }
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
            SourceList.Clear();
            _allItems.Clear();
            
            var products = await _dataService.GetAllProducts(forceRefresh: true);
            foreach (var p in products)
            {
                _allItems.Add(new InventoryItem
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Inventory = p.Inventory,
                    TemporaryInventory = p.Inventory,
                    Source = "PRODUCTS"
                });
            }

            var materials = await _dataService.GetMaterials(forceRefresh: true);
            foreach (var m in materials)
            {
                _allItems.Add(new InventoryItem
                {
                    ProductID = m.ProductID,
                    Name = m.Name,
                    Inventory = m.Inventory,
                    TemporaryInventory = m.Inventory,
                    Source = "MATERIALS"
                });
            }

            UpdateTemporaryInventories(); // Apply any existing transactions if necessary (though usually empty on load)

            foreach (var item in _allItems) SourceList.Add(item);
        }, "Load data failed");
    }

    public async Task SaveData()
    {
        await ExecuteAsync(async () =>
        {
            var transactions = TransactionList.ToList();

            foreach (var trans in transactions)
            {
                trans.CreatedDate = DateTime.Now;
                if (string.IsNullOrEmpty(trans.InvoiceID))
                {
                    trans.InvoiceID = null;
                }
                
                var originalItem = _allItems.FirstOrDefault(x => x.ProductID == trans.ProductID);
                if (originalItem != null)
                {
                    trans.SourceType = originalItem.Source == "PRODUCTS" ? "PRODUCT" : "MATERIAL";
                }

                await _dataService.AddWarehouseTransaction(trans);

                var localItem = _allItems.FirstOrDefault(x => x.ProductID == trans.ProductID);
                if (localItem != null)
                {
                    localItem.Inventory += trans.FinalChange;
                    localItem.TemporaryInventory = localItem.Inventory;
                }
            }

            TransactionList.Clear();
            UpdateTemporaryInventories();

            await DialogService.ShowSuccessAsync("SUCCESS_SAVE".GetLocalized());
        }, "Lỗi lưu kho");
    }

    public void Search(string keyword)
    {
        SourceList.Clear();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            foreach (var item in _allItems) SourceList.Add(item);
        }
        else
        {
            var filtered = _allItems.Where(x => x.Name != null && x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            );

            foreach (var item in filtered) SourceList.Add(item);
        }
    }

    public int GetCurrentInventory(long? productId)
    {
        if (productId == null) return 0;
        var item = _allItems.FirstOrDefault(x => x.ProductID == productId);
        return item != null ? item.Inventory : 0;
    }
}
