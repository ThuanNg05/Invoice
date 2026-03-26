using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class MaterialsViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public List<Materials> AllMaterials = [];
    public ObservableCollection<Materials> MaterialsCollection { get; } = [];

    public MaterialsViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.MATERIALS)
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
            MaterialsCollection.Clear();
            var data = await _dataService.GetMaterials(forceRefresh: true);
            AllMaterials = data.ToList();
            foreach (var item in AllMaterials)
            {
                MaterialsCollection.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task AddMaterialAsync(Materials material)
    {
        var existing = await _dataService.GetMaterialByName(material.Name);
        if (existing != null)
        {
            var result = await DialogService.ShowConfirmAsync("Thông báo", $"Vật tư '{material.Name}' đã tồn tại trong hệ thống. Bạn có muốn phục hồi và thay thế dữ liệu mới không?");
            if (result)
            {
                await ExecuteAsync(async () =>
                {
                    await _dataService.HardDeleteMaterial(existing.ProductID);
                    await _dataService.AddMaterial(material);
                    
                    var currentInList = MaterialsCollection.FirstOrDefault(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase));
                    if (currentInList != null)
                    {
                        MaterialsCollection.Remove(currentInList);
                        AllMaterials.RemoveAll(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase));
                    }

                    MaterialsCollection.Add(material);
                    AllMaterials.Add(material);
                    await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
                }, "Lỗi khi phục hồi vật tư");
            }
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddMaterial(material);
            MaterialsCollection.Add(material);
            AllMaterials.Add(material);
            await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
        }, "Lỗi thêm vật tư");
    }

    public async Task DeleteMaterialAsync(Materials material)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeleteMaterial(material.ProductID);
            MaterialsCollection.Remove(material);
            var itemAll = AllMaterials.FirstOrDefault(m => m.ProductID == material.ProductID);
            if (itemAll != null) AllMaterials.Remove(itemAll);
            await DialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
        }, "Lỗi xoá vật tư");
    }

    public async Task UpdateMaterialAsync(Materials material)
    {
        if (MaterialsCollection.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase) && m.ProductID != material.ProductID))
        {
            await DialogService.ShowErrorAsync("Tên vật tư này đã tồn tại. Vui lòng nhập tên khác");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.UpdateMaterial(material);

            var itemToUpdate = MaterialsCollection.FirstOrDefault(c => c.ProductID == material.ProductID);
            if (itemToUpdate != null)
            {
                var index = MaterialsCollection.IndexOf(itemToUpdate);
                if (index != -1)
                {
                    MaterialsCollection.RemoveAt(index);
                    MaterialsCollection.Insert(index, material);
                }
            }

            var itemInAll = AllMaterials.FirstOrDefault(c => c.ProductID == material.ProductID);
            if (itemInAll != null)
            {
                var indexAll = AllMaterials.IndexOf(itemInAll);
                if (indexAll != -1)
                {
                    AllMaterials[indexAll] = material;
                }
            }
            await DialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }, "Lỗi cập nhật vật tư");
    }
}
