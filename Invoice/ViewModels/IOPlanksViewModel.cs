using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;

namespace Invoice.ViewModels;

public partial class IOPlanksViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private bool isLoading;

    private readonly IDataService _dataService;

    public ObservableCollection<Frames> FramesSource { get; } = new ObservableCollection<Frames>();

    [ObservableProperty]
    private Frames? selectedFrame;

    public IOPlanksViewModel(IDataService dataService)
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
        FramesSource.Clear();
        var data = await _dataService.GetFrames(forceRefresh: false);
        foreach (var item in data)
        {
            FramesSource.Add(item);
        }
        IsLoading = false;
    }

    public void OnNavigatedFrom()
    {
    }

    public async Task SaveTransaction(Frames frames, int amount)
    {

    }
}