using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class PlanksViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<Frames> Frames { get; } = new ObservableCollection<Frames>();

    public PlanksViewModel(IDataService dataService)
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
        Frames.Clear();
        var data = await _dataService.GetFrames(forceRefresh: false);
        foreach (var item in data)
        {
            Frames.Add(item);
        }
        IsLoading = false;

    }

    public void OnNavigatedFrom()
    {
    }

    public async Task AddFrameAsync(Frames frame)
    {
        if (Frames.Any(p => p.FrameNO.Equals(frame.FrameNO, StringComparison.OrdinalIgnoreCase)))
        {
            Debug.WriteLine("Lỗi: Mã ván đã tồn tại!");
            await App.ShowMessageAsync("Lỗi", "Mã ván đã tồn tại!");
            return;
        }
        try
        {
            await _dataService.AddFrame(frame);
            Frames.Add(frame);
            await App.ShowMessageAsync("Thông báo", "Đã thêm rập mới.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi thêm rập: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể thêm rập mới.");
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteFrameAsync(Frames frame)
    {
        IsLoading = true;
        try
        {
            await _dataService.DeleteFrame(frame.FrameID);
            Frames.Remove(frame);
            await App.ShowMessageAsync("Thông báo", "Đã xóa rập.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi xóa rập: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể xóa rập.");
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateFrameAsync(Frames frame)
    {
        IsLoading = true;
        try
        {
            await _dataService.UpdateFrame(frame);

            var itemToUpdate = Frames.FirstOrDefault(c => c.FrameID == frame.FrameID);
            if (itemToUpdate != null)
            {
                var index = Frames.IndexOf(itemToUpdate);
                if (index != -1)
                {
                    Frames.RemoveAt(index);
                    Frames.Insert(index, frame);
                }
            }
            await App.ShowMessageAsync("Thông báo", "Đã cập nhật rập.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Lỗi khi cập nhật rập: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Không thể cập nhật rập.");
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }
}