using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class DetailPlanksViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<DetailPlanks> Planks { get; } = new ObservableCollection<DetailPlanks>();

    public DetailPlanksViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        IsLoading = true;

        Planks.Clear();
        try
        {
            var data = await _dataService.GetPlanks();
            foreach (var item in data)
            {
                Planks.Add(item);
            }
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("Failed to load Detail planks data.");
            await App.ShowMessageAsync("Lỗi", "Không thể tải dữ liệu cỡ ván.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task AddPlankAsync(DetailPlanks planks)
    {
        if (Planks.Any(c => c.sizeID.Equals(planks.sizeID, StringComparison.OrdinalIgnoreCase)))
        {
            System.Diagnostics.Debug.WriteLine("Lỗi: kích thước này đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Kích thước này đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.AddPlank(planks);
            Planks.Add(planks);
            await App.ShowMessageAsync("Thông báo", "Thêm kích thước thành công.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi thêm: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", $"{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task DeletePlankAsync(DetailPlanks planks)
    {
        IsLoading = true;
        try
        {
            await _dataService.DeletePlank(planks.sizeID);
            Planks.Remove(planks);
            await App.ShowMessageAsync("Thông báo", "Xóa kích thước thành công.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi xóa: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", $"{ex.Message}");
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task UpdatePlankAsync(DetailPlanks planks)
    {
        if (Planks.Any(c => c.sizeID.Equals(planks.sizeID, StringComparison.OrdinalIgnoreCase)))
        {
            System.Diagnostics.Debug.WriteLine("Lỗi: Kích thước này đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Kích thước này đã tồn tại!");
            return;
        }
        IsLoading = true;
        try
        {
            await _dataService.UpdatePlank(planks);

            var item = Planks.FirstOrDefault(c => c.sizeID == planks.sizeID);
            if (item != null)
            {
                item.sizeID = planks.sizeID;
            }
            await App.ShowMessageAsync("Thông báo", "Cập nhật kích thước thành công.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi sửa: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", $"{ex.Message}");
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }
}