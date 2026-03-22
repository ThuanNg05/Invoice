using Invoice.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class HistoryTransactionPage : Page
{
    public HistoryTransactionViewModel ViewModel
    {
        get;
    }

    public HistoryTransactionPage()
    {
        ViewModel = App.GetService<HistoryTransactionViewModel>();
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
    }
}
