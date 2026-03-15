using Invoice.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class ProductSelectionPage : Page
{
    public ProductSelectionViewModel ViewModel
    {
        get;
    }

    public ProductSelectionPage()
    {
        ViewModel = App.GetService<ProductSelectionViewModel>();
        InitializeComponent();
    }
}
