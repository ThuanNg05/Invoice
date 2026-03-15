using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Invoice.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SplashScreenWindow : Window
{
    public SplashScreenWindow()
    {
        InitializeComponent();
        var appWindow = this.AppWindow;

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        appWindow.Resize(new Windows.Graphics.SizeInt32(600, 400));
        CenterWindow(appWindow);
    }

    private void CenterWindow(AppWindow appWindow)
    {
        var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
        var screenWidth = displayArea.WorkArea.Width;
        var screenHeight = displayArea.WorkArea.Height;
        var windowWidth = appWindow.Size.Width;
        var windowHeight = appWindow.Size.Height;
        int x = (screenWidth - windowWidth) / 2;
        int y = (screenHeight - windowHeight) / 2;
        appWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }
}