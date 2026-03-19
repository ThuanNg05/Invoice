using Invoice.Contracts.Services;
using Invoice.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Services;

public class DialogService : IDialogService
{
    private XamlRoot? GetXamlRoot()
    {
        if (App.MainWindow?.Content is FrameworkElement element)
        {
            return element.XamlRoot;
        }
        return null;
    }

    public async Task ShowMessageAsync(string title, string content)
    {
        var xamlRoot = GetXamlRoot();
        if (xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "Common_Close".GetLocalized(),
            XamlRoot = xamlRoot
        };

        try
        {
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dialog error: {ex.Message}");
        }
    }

    public Task ShowSuccessAsync(string content)
        => ShowMessageAsync("Common_Success".GetLocalized(), content);

    public Task ShowErrorAsync(string content, Exception? ex = null)
        => ShowMessageAsync("Common_Error".GetLocalized(), ex == null ? content : $"{content}\nChi tiết: {ex.Message}");

    public async Task<bool> ShowConfirmAsync(string title, string content, string? primaryButton = null)
    {
        var xamlRoot = GetXamlRoot();
        if (xamlRoot == null) return false;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButton ?? "Common_Confirm".GetLocalized(),
            CloseButtonText = "Common_Cancel".GetLocalized(),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
