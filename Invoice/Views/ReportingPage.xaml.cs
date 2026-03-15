using Invoice.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Invoice.Views;

public sealed partial class ReportingPage : Page
{
    public ReportingViewModel ViewModel
    {
        get;
    }

    public ReportingPage()
    {
        ViewModel = App.GetService<ReportingViewModel>();
        InitializeComponent();
    }
}
