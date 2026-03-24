using System.Collections.ObjectModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class PlanksViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public ObservableCollection<Frames> Frames { get; } = new ObservableCollection<Frames>();

    public ObservableCollection<DetailPlanks> Planks { get; } = new ObservableCollection<DetailPlanks>();

    public PlanksViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
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
            Frames.Clear();
            var data = await _dataService.GetFrames(forceRefresh: true);
            foreach (var item in data)
            {
                Frames.Add(item);
            }

            Planks.Clear();
            var planks = await _dataService.GetPlanks();
            foreach (var item in planks)
            {
                Planks.Add(item);
            }
        }, "Lỗi khi tải dữ liệu");
    }

    public async Task ProcessTransactionAsync(Frames frame, int amount, long materialID)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.ProcessInventoryTransaction(frame, amount, materialID);
            await DialogService.ShowSuccessAsync("Nhập kho thành công");
        }, "Lỗi khi nhập kho");
    }

    public async Task AddFrameAsync(Frames frame)
    {
        if (Frames.Any(p => p.FrameNO.Equals(frame.FrameNO, StringComparison.OrdinalIgnoreCase)))
        {
            await DialogService.ShowErrorAsync("Mã ván đã tồn tại!");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddFrame(frame);
            Frames.Add(frame);
            await DialogService.ShowSuccessAsync("SUCCESS_ADD".GetLocalized());
        }, "FAILED_ADD".GetLocalized());
    }

    public async Task DeleteFrameAsync(Frames frame)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeleteFrame(frame.FrameID);
            Frames.Remove(frame);
            await DialogService.ShowSuccessAsync("SUCCESS_DELETE".GetLocalized());
        }, "Lỗi khi xóa ván");
    }

    public async Task UpdateFrameAsync(Frames frame)
    {
        await ExecuteAsync(async () =>
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
            await DialogService.ShowSuccessAsync("SUCCESS_UPDATE".GetLocalized());
        }, "FAILED_UPDATE".GetLocalized());
    }
}
