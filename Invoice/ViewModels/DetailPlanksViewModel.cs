using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Invoice.Contracts.Services;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class DetailPlanksViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public ObservableCollection<DetailPlanks> Planks { get; } = new ObservableCollection<DetailPlanks>();

    public DetailPlanksViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;

        WeakReferenceMessenger.Default.Register<DatabaseChangedMessage>(this, (r, m) =>
        {
            if (m.EntityName == InMemoryCache.PLANKS)
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
            Planks.Clear();
            var data = await _dataService.GetPlanks(forceRefresh: true);
            foreach (var item in data)
            {
                Planks.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task AddPlankAsync(DetailPlanks planks)
    {
        var existing = await _dataService.GetPlankByName(planks.sizeID);
        if (existing != null)
        {
            var result = await DialogService.ShowConfirmAsync("Thông báo", $"Cỡ ván '{planks.sizeID}' đã tồn tại trong hệ thống. Bạn có muốn phục hồi và thay thế dữ liệu mới không?");
            if (result)
            {
                await ExecuteAsync(async () =>
                {
                    await _dataService.HardDeletePlank(existing.sizeID);
                    await _dataService.AddPlank(planks);
                    
                    var currentInList = Planks.FirstOrDefault(p => p.sizeID.Equals(planks.sizeID, StringComparison.OrdinalIgnoreCase));
                    if (currentInList != null)
                    {
                        Planks.Remove(currentInList);
                    }

                    Planks.Add(planks);
                    await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
                }, "Lỗi khi phục hồi cỡ ván");
            }
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddPlank(planks);
            Planks.Add(planks);
            await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
        }, "Lỗi thêm cỡ ván");
    }

    public async Task DeletePlankAsync(DetailPlanks planks)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeletePlank(planks.sizeID);
            Planks.Remove(planks);
            await DialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
        }, "Lỗi xoá ván");
    }

    public async Task UpdatePlankAsync(DetailPlanks planks)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.UpdatePlank(planks);
            await DialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }, "Lỗi cập nhật ván");
    }
}
