using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class IOActionsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    private List<InventoryItem> _allItems = new();
    public ObservableCollection<InventoryItem> SourceList { get; } = new();
    public ObservableCollection<WarehouseTransaction> TransactionList { get; } = new();

    public IOActionsViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void OnNavigatedTo(object parameter)
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
        }
    }

    private async Task LoadData()
    {
        SourceList.Clear();
        _allItems.Clear();
        var products = await _dataService.GetAllProducts();
        foreach (var p in products)
        {
            _allItems.Add(new InventoryItem
            {
                ProductID = p.ProductID,
                Name = p.Name,
                Inventory = p.Inventory,
                Source = "PRODUCTS"
            });
        }

        var materials = await _dataService.GetMaterials();
        foreach (var m in materials)
        {
            _allItems.Add(new InventoryItem
            {
                ProductID = m.ProductID,
                Name = m.Name,
                Inventory = m.Inventory,
                Source = "MATERIALS"
            });
        }

        foreach (var item in _allItems) SourceList.Add(item);
    }

    public async Task SaveData()
    {
        var transactions = TransactionList.ToList();

        foreach (var trans in transactions)
        {
            trans.CreatedDate = DateTime.Now;
            if (string.IsNullOrEmpty(trans.InvoiceID))
            {
                trans.InvoiceID = null;
            }

            await _dataService.AddWarehouseTransaction(trans);

            var localItem = _allItems.FirstOrDefault(x => x.ProductID == trans.ProductID);
            if (localItem != null)
            {
                localItem.Inventory += trans.FinalChange;
            }
        }

        TransactionList.Clear();
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
            var filtered = _allItems.Where(x =>
                x.ProductID.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (x.Name != null && x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            );

            foreach (var item in filtered) SourceList.Add(item);
        }
    }

    public int GetCurrentInventory(long productId)
    {
        var item = _allItems.FirstOrDefault(x => x.ProductID == productId);
        return item != null ? item.Inventory : 0;
    }


}