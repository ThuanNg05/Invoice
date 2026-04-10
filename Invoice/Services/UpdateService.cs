using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Invoice.Contracts.Services;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Services;

public class UpdateService : IUpdateService
{
    private readonly IDialogService _dialogService;
    private readonly HttpClient _httpClient;
    private const string RepoUrl = "https://api.github.com/repos/ThuanNg05/Invoice/releases/latest";

    // TODO: Update this URL to point to the webpage where users can download the latest release
    private const string DownloadUrl = "https://github.com/ThuanNg05/Invoice/releases/latest";

    public UpdateService(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _httpClient = new HttpClient();
        // GitHub API requires a User-Agent header
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "InvoiceApp-UpdateChecker");
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            var latestRelease = await _httpClient.GetFromJsonAsync<GitHubRelease>(RepoUrl);
            if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName)) return;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion == null) return;

            // Strip 'v' from tag name if present (e.g., "v1.0.1" -> "1.0.1")
            var latestVersionStr = latestRelease.TagName.TrimStart('v');
            if (Version.TryParse(latestVersionStr, out var latestVersion))
            {
                if (latestVersion > currentVersion)
                {
                    await ShowUpdateDialog(latestRelease.TagName, latestRelease.Body);
                }
            }
        }
        catch (Exception ex)
        {
            // Silently fail for update checks, or log it
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
        }
    }

    private async Task ShowUpdateDialog(string tagName, string body)
    {
        var result = await _dialogService.ShowTwoOptionsAsync(
            "Cập nhật mới",
            $"Đã có phiên bản mới: {tagName}\n\n{body}\n\nBạn có muốn tải về ngay không?",
            "Tải ngay",
            "Tải sau"
        );

        if (result == true)
        {
            // Open the release page in the default browser
            await Windows.System.Launcher.LaunchUriAsync(new Uri(DownloadUrl));
        }
    }

    private class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}
