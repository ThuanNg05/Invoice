using Invoice.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class MaterialsPage : Page
{
    public MaterialsViewModel ViewModel
    {
        get;
    }

    public MaterialsPage()
    {
        ViewModel = App.GetService<MaterialsViewModel>();
        InitializeComponent();
    }
}
