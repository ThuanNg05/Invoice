using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class MaterialsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    public List<Materials> AllMaterials = new();
    public ObservableCollection<Materials> MaterialsCollection { get; } = new();

    public MaterialsViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        _ = LoadDataSafeAsync();
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
        IsLoading = true;
        MaterialsCollection.Clear();
        var data = await _dataService.GetMaterials(forceRefresh: false);
        AllMaterials = data.ToList();
        foreach (var item in data)
        {
            MaterialsCollection.Add(item);
        }
        IsLoading = false;
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task AddMaterialAsync(Materials material)
    {
        if (MaterialsCollection.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Debug.WriteLine("Lỗi: Tên vật tư đã tồn tại!");            
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.AddMaterial(material);
            MaterialsCollection.Add(material);
            AllMaterials.Add(material);            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to add material: {ex.Message}");            
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteMaterialAsync(Materials material)
    {
        IsLoading = true;
        try
        {
            await _dataService.DeleteMaterial(material.ProductID);
            MaterialsCollection.Remove(material);
            var itemAll = AllMaterials.FirstOrDefault(m => m.ProductID == material.ProductID);
            if (itemAll != null) AllMaterials.Remove(itemAll);            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete material: {ex.Message}");            
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateMaterialAsync(Materials material)
    {
        if (MaterialsCollection.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase) && m.ProductID != material.ProductID))
        {
            Debug.WriteLine("Lỗi: Tên vật tư đã tồn tại!");            
            return;
        }
        IsLoading = true;
        try
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to update material: {ex.Message}");            
        }
        finally
        {
            IsLoading = false;
        }
    }
}