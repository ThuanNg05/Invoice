using System.Collections.ObjectModel;
using Invoice.Contracts.ViewModels;
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
            var data = await _dataService.GetMaterials(forceRefresh: false);
            AllMaterials = data.ToList();
            foreach (var item in AllMaterials)
            {
                MaterialsCollection.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task AddMaterialAsync(Materials material)
    {
        if (MaterialsCollection.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase)))
        {
            await DialogService.ShowErrorAsync("Tên vật tư này đã tồn tại. Vui lòng nhập tên khác");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddMaterial(material);
            MaterialsCollection.Add(material);
            AllMaterials.Add(material);
            await DialogService.ShowSuccessAsync("SUCESS_ADD".GetLocalized());
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
            await DialogService.ShowSuccessAsync("SUCESS_DELETE".GetLocalized());
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
            await DialogService.ShowSuccessAsync("SUCESS_UPDATE".GetLocalized());
        }, "Lỗi cập nhật vật tư");
    }
}
