using Invoice.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class ProductsPage : Page
{
    public ProductsViewModel ViewModel
    {
        get;
    }

    public ProductsPage()
    {
        ViewModel = App.GetService<ProductsViewModel>();
        InitializeComponent();
    }
}
