using Invoice.Activation;
using Invoice.Contracts.Services;
using Invoice.Core.Contracts.Services;
using Invoice.Core.Services;
using Invoice.Helpers;
using Invoice.Models;
using Invoice.Services;
using Invoice.ViewModels;
using Invoice.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Windows.Globalization;

namespace Invoice;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        ApplicationLanguages.PrimaryLanguageOverride = "vi-VN";
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }).
        ConfigureServices((context, services) =>
        {
            // Supabase service
            var supabaseUrl = context.Configuration["Supabase:Url"];
            var supabaseKey = context.Configuration["Supabase:Key"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
            {
                throw new InvalidOperationException("Supabase configuration is missing. Please check appsettings.json.");
            }

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddSingleton<Supabase.Client>(provider =>
            {
                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                };

                return new Supabase.Client(supabaseUrl, supabaseKey, options);
            });

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services            
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<PlanksViewModel>();
            services.AddTransient<PlanksPage>();
            services.AddTransient<IOPlanksViewModel>();
            services.AddTransient<IOPlanksPage>();
            services.AddTransient<ReportingViewModel>();
            services.AddTransient<ReportingPage>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<ProductSelectionViewModel>();
            services.AddTransient<ProductSelectionPage>();
            services.AddTransient<MaterialsViewModel>();
            services.AddTransient<MaterialsPage>();
            services.AddTransient<IOActionsViewModel>();
            services.AddTransient<IOActionsPage>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<HistoryPage>();
            services.AddTransient<EditingInvoiceViewModel>();
            services.AddTransient<EditingInvoicePage>();
            services.AddTransient<DetailPriceViewModel>();
            services.AddTransient<DetailPricePage>();
            services.AddTransient<DetailPlanksViewModel>();
            services.AddTransient<DetailPlanksPage>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomersPage>();
            services.AddTransient<CreateInvoiceViewModel>();
            services.AddTransient<CreateInvoicePage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();            
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        var splash = new SplashScreenWindow();
        splash.Activate();

        await Task.Delay(3000);

        await App.GetService<IActivationService>().ActivateAsync(args);

        splash.Close();
    }

    public static async Task ShowMessageAsync(string title, string content)
    {
        // Kiểm tra xem MainWindow có Content không để lấy XamlRoot
        if (MainWindow.Content is FrameworkElement element)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Đóng",
                XamlRoot = element.XamlRoot // Quan trọng: WinUI 3 yêu cầu phải gán XamlRoot
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch
            {
                // Bỏ qua lỗi nếu có dialog khác đang mở
            }
        }
    }
}
