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

    private void OnPasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.PasswordInput = passwordBox.Password;
        }
    }
}
