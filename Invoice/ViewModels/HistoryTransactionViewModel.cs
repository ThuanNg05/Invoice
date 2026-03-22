using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Contracts.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Contracts.Services;
using Invoice.Helpers;

namespace Invoice.ViewModels;

public partial class HistoryTransactionViewModel : ViewModelBase, INavigationAware
{
    private readonly IDataService _dataService;

    public ObservableCollection<WarehouseHistoryItem> Source { get; } = new ObservableCollection<WarehouseHistoryItem>();

    public List<string> SourceTypes { get; } = new() { "Sản phẩm", "Nguyên liệu" };

    [ObservableProperty]
    private string? _selectedSourceType; // Mặc định null (không chọn)

    [ObservableProperty]
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    public HistoryTransactionViewModel(IDataService dataService, IDialogService dialogService) : base(dialogService)
    {
        _dataService = dataService;
        StartDate = null;
        EndDate = null;
        _selectedSourceType = null;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await ResetQuery();
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    public async Task SearchInvoice()
    {
        await ExecuteAsync(async () =>
        {
            Source.Clear();
            DateTime? from = StartDate?.DateTime;
            DateTime? to = EndDate?.DateTime;
            
            string? sourceTypeFilter = null;
            if (SelectedSourceType == "Sản phẩm") sourceTypeFilter = "PRODUCT";
            else if (SelectedSourceType == "Nguyên liệu") sourceTypeFilter = "MATERIAL";

            var historyData = await _dataService.GetQueryableHistory(from, to, sourceTypeFilter);

            foreach (var item in historyData)
            {
                Source.Add(item);
            }
        }, "LOAD_FAILED".GetLocalized());
    }

    [RelayCommand]
    public async Task ResetQuery()
    {
        SelectedSourceType = null;
        StartDate = null;
        EndDate = null;
        Source.Clear();
        await Task.CompletedTask;
    }
}
