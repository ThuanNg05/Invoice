using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class DetailPlanksViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public ObservableCollection<DetailPlanks> Planks { get; } = new ObservableCollection<DetailPlanks>();

    public DetailPlanksViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
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
            Planks.Clear();
            var data = await _dataService.GetPlanks();
            foreach (var item in data)
            {
                Planks.Add(item);
            }
        }, "Planks_Error_Load".GetLocalized());
    }

    public async Task AddPlankAsync(DetailPlanks planks)
    {
        if (Planks.Any(c => c.sizeID.Equals(planks.sizeID, StringComparison.OrdinalIgnoreCase)))
        {
            await DialogService.ShowErrorAsync("Planks_Error_Duplicate".GetLocalized());
            return;
        }

        await ExecuteAsync(async () =>
        {
            await _dataService.AddPlank(planks);
            Planks.Add(planks);
        }, "Planks_Error_Add".GetLocalized());
    }

    public async Task DeletePlankAsync(DetailPlanks planks)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.DeletePlank(planks.sizeID);
            Planks.Remove(planks);
        }, "Planks_Error_Delete".GetLocalized());
    }

    public async Task UpdatePlankAsync(DetailPlanks planks)
    {
        await ExecuteAsync(async () =>
        {
            await _dataService.UpdatePlank(planks);
        }, "Planks_Error_Update".GetLocalized());
    }
}
