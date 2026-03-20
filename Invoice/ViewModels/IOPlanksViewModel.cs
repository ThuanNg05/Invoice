using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class IOPlanksViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public ObservableCollection<Frames> FramesSource { get; } = new ObservableCollection<Frames>();

    [ObservableProperty]
    private Frames? selectedFrame;

    public IOPlanksViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
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
            FramesSource.Clear();
            var data = await _dataService.GetFrames(forceRefresh: false);
            foreach (var item in data)
            {
                FramesSource.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    public async Task SaveTransaction(Frames frames, int amount)
    {
        await Task.CompletedTask;
    }
}
