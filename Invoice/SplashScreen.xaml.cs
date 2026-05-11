using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Invoice;

public sealed partial class SplashScreen : Window
{    
    public SplashScreen()
    {
        this.InitializeComponent();
        IntPtr hwd = WindowNative.GetWindowHandle(this);
        WindowId WndID = Win32Interop.GetWindowIdFromWindow(hwd);
        AppWindow appW = AppWindow.GetFromWindowId(WndID);
        OverlappedPresenter presenter = appW.Presenter as OverlappedPresenter;        
        presenter.IsAlwaysOnTop = true;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;                
        appW.Resize(new Windows.Graphics.SizeInt32 { Width = 620, Height = 300});        
        presenter.SetBorderAndTitleBar(false, false);        
                
        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x00080000;
        const uint LWA_COLORKEY = 0x00000001;

        int exStyle = GetWindowLong(hwd, GWL_EXSTYLE);
        SetWindowLong(hwd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);

        // RGB(255, 0, 255) = Magenta => vùng nào có màu này sẽ transparent
        uint colorKey = 0x00000000;
        SetLayeredWindowAttributes(hwd, colorKey, 0, LWA_COLORKEY);
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetLayeredWindowAttributes(
        IntPtr hwnd,
        uint crKey,
        byte bAlpha,
        uint dwFlags);

    public async Task SetWindowPositionToCenter()
    {
        var hwd = WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var centeredPosition = appWindow.Position;
                centeredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                centeredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                appWindow.Move(centeredPosition);
            }
        }

        await Task.CompletedTask;
    }
}
