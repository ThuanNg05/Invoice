using Invoice.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Invoice.Views;

public class EditingInvoiceNavigationParameter
{
    public Action<string> OnInvoiceSelected
    {
        get; set;
    }
}

public sealed partial class EditingInvoicePage : Page
{
    public EditingInvoiceViewModel ViewModel
    {
        get;
    }

    public EditingInvoicePage()
    {
        ViewModel = App.GetService<EditingInvoiceViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is EditingInvoiceNavigationParameter param)
        {
            ViewModel.OnInvoiceConfirmed = param.OnInvoiceSelected;
        }
        await ViewModel.LoadInvoiceListAsync();
    }
}