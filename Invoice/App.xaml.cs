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
        System.Diagnostics.Debug.WriteLine("App Constructor started.");
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        InitializeComponent();
        
        System.Diagnostics.Debug.WriteLine("Building Host...");          
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
            var supabaseKey = context.Configuration.GetRequiredDecrypted("Supabase:Key");

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
                    AutoConnectRealtime = false
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
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IDialogService, DialogService>();

            // Core Services            
            services.AddSingleton<SupabaseDataService>();
            services.AddSingleton<IDataService>(sp => sp.GetRequiredService<SupabaseDataService>());
            services.AddSingleton<ICustomerService>(sp => sp.GetRequiredService<SupabaseDataService>());
            services.AddSingleton<IProductService>(sp => sp.GetRequiredService<SupabaseDataService>());
            services.AddSingleton<IInvoiceService>(sp => sp.GetRequiredService<SupabaseDataService>());
            services.AddSingleton<IInventoryService>(sp => sp.GetRequiredService<SupabaseDataService>());
                
            services.AddSingleton<IFileService, FileService>();

            // App Services
            services.AddSingleton<InvoicePdfService>();
            services.AddSingleton<ReportingService>();
            services.AddSingleton<ReportPdfService>();
            services.AddSingleton<EmailService>();

            // Views and ViewModels
            services.AddTransient<HistoryTransactionViewModel>();
            services.AddTransient<HistoryTransactionPage>();
            services.AddTransient<PlanksViewModel>();
            services.AddTransient<PlanksPage>();
            services.AddTransient<IOActionsViewModel>();
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

        System.Diagnostics.Debug.WriteLine("Host built successfully.");
            
        UnhandledException += App_UnhandledException;        
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var error = $"UNHANDLED EXCEPTION: {e.Message}\nDETAILS: {e.Exception}";
        System.Diagnostics.Debug.WriteLine(error);
        System.Diagnostics.Trace.WriteLine(error);
        e.Handled = true;

        try
        {
            await GetService<IDialogService>().ShowMessageAsync("Lỗi hệ thống", $"Đã xảy ra lỗi không mong muốn: {e.Message}");
        }
        catch
        {
            // Fallback if dialog cannot be shown
        }
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {                        
            // ApplicationLanguages.PrimaryLanguageOverride = "vi-VN"; // Causes crash in WinUI 3
            base.OnLaunched(args);                       

            System.Diagnostics.Debug.WriteLine("Activating services...");
            await App.GetService<IActivationService>().ActivateAsync(args);
            System.Diagnostics.Debug.WriteLine("Services activated.");
            
        }
        catch (Exception ex)
        {
            var error = $"LAUNCH ERROR: {ex.Message}\nSTACKTRACE: {ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine(error);
            System.Diagnostics.Trace.WriteLine(error);
            await GetService<IDialogService>().ShowMessageAsync("Lỗi khởi động", $"Có lỗi xảy ra khi khởi động ứng dụng: {ex.Message}");
        }
    }
}
