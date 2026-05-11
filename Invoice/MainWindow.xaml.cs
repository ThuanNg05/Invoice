using Invoice.Helpers;

using Windows.UI.ViewManagement;

namespace Invoice;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        if (AppWindow != null)
        {
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Warning: AppWindow is null in MainWindow constructor.");
        }

        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
