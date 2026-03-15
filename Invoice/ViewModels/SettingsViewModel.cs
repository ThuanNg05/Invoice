using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invoice.Contracts.Services;
using Invoice.Helpers;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;

namespace Invoice.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private const string InvoiceStorageKey = "InvoiceStoragePath";

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string _invoiceStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        var savedPath = await _localSettingsService.ReadSettingAsync<string>(InvoiceStorageKey);
        if (!string.IsNullOrEmpty(savedPath))
        {
            InvoiceStoragePath = savedPath;
        }
    }

    [RelayCommand]
    private async Task SelectStorageFolder()
    {
        try
        {
            // Tạo FolderPicker
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");

            // QUAN TRỌNG: Lấy Window Handle (HWND) để FolderPicker hoạt động trong WinUI 3
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            // Mở dialog chọn folder
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Lưu đường dẫn vào biến và LocalSettings
                InvoiceStoragePath = folder.Path;
                await _localSettingsService.SaveSettingAsync(InvoiceStorageKey, folder.Path);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi chọn thư mục: {ex.Message}");
            await App.ShowMessageAsync("Lỗi", "Đã xảy ra lỗi khi chọn thư mục lưu trữ.");
        }
    }

    async partial void OnElementThemeChanged(ElementTheme value)
    {
        await _themeSelectorService.SetThemeAsync(value);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}