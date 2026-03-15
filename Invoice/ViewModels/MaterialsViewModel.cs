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

    public ObservableCollection<Materials> Materials { get; } = new();

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
            await App.ShowMessageAsync("Lỗi", "Không thể tải dữ liệu.");
        }
    }

    private async Task LoadData()
    {
        IsLoading = true;
        Materials.Clear();
        var data = await _dataService.GetMaterials(forceRefresh: false);
        foreach (var item in data)
        {
            Materials.Add(item);
        }
        IsLoading = false;
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task AddMaterialAsync(Materials material)
    {
        if (Materials.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Debug.WriteLine("Lỗi: Tên vật tư đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Tên vật tư đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.AddMaterial(material);
            Materials.Add(material);
            await App.ShowMessageAsync("Thông báo", "Vật tư đã được thêm thành công.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to add material: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể thêm vật tư.");
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
            Materials.Remove(material);
            await App.ShowMessageAsync("Thông báo", "Vật tư đã được xóa thành công.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete material: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể xóa vật tư.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateMaterialAsync(Materials material)
    {
        if (Materials.Any(m => m.Name.Equals(material.Name, StringComparison.OrdinalIgnoreCase) && m.ProductID != material.ProductID))
        {
            Debug.WriteLine("Lỗi: Tên vật tư đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Tên vật tư đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.UpdateMaterial(material);

            var itemToUpdate = Materials.FirstOrDefault(c => c.ProductID == material.ProductID);

            if (itemToUpdate != null)
            {
                var index = Materials.IndexOf(itemToUpdate);
                if (index != -1)
                {
                    Materials.RemoveAt(index);
                    Materials.Insert(index, material);
                }
            }
            await App.ShowMessageAsync("Thông báo", "Vật tư đã được cập nhật thành công.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to update material: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể cập nhật vật tư.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}