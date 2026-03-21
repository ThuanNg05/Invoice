using System.Collections.ObjectModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;

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

                await _dataService.AddWarehouseTransaction(trans);

                var localItem = _allItems.FirstOrDefault(x => x.ProductID == trans.ProductID);
                if (localItem != null)
                {
                    localItem.Inventory += trans.FinalChange;
                }
            }

            TransactionList.Clear();            
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

    public int GetCurrentInventory(long productId)
    {
        var item = _allItems.FirstOrDefault(x => x.ProductID == productId);
        return item != null ? item.Inventory : 0;
    }
}
