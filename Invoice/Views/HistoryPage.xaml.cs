using Invoice;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Models;
using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class HistoryPage : Page
{
    private readonly IDataService _dataService;
    public HistoryViewModel ViewModel
    {
        get;
    }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        _dataService = App.GetService<IDataService>();
        InitializeComponent();
        this.DataContext = ViewModel;
    }

    private async void BtnQuery_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SearchInvoiceCommand.ExecuteAsync(null);
    }

    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResetQueryCommand.ExecuteAsync(null);
        asbCustomer.Text = string.Empty;
    }

    private void asbCustomer_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.SelectedCustomer = null;

            var query = sender.Text.ToLower();
            var filteredList = ViewModel.CustomersList
                                .Where(c => c.Name != null && c.Name.ToLower().Contains(query))
                                .ToList();
            sender.ItemsSource = filteredList;
        }
    }

    private async void asbCustomer_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Customers selectedCustomer)
        {
            ViewModel.SelectedCustomer = selectedCustomer;
            sender.Text = selectedCustomer.Name;
        }
        else
        {
            ViewModel.SelectedCustomer = null;
        }
    }
}