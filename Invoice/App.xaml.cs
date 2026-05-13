using System.Diagnostics;
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

    public static WindowEx MainWindow { get; private set; }
    private SplashScreen splash_screen;

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        UnhandledException += App_UnhandledException;

        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            LogFatalError("InitializeComponent failed", ex);
            throw;
        }

        try
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "debug_log.txt");
            Trace.Listeners.Add(new TextWriterTraceListener(logPath));
            Trace.AutoFlush = true;
            Debug.WriteLine($"=== Application Started at {DateTime.Now} ===");
        }
        catch { /* Fallback if logging fails */ }
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        //ApplicationLanguages.PrimaryLanguageOverride = "vi-VN";
        var culture = new System.Globalization.CultureInfo("vi-VN");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        Debug.WriteLine("Building Host...");          
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
            services.AddSingleton<IUpdateService, UpdateService>();

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
    }

    private void LogFatalError(string context, Exception ex)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "fatal_error.txt");
        File.AppendAllText(logPath, $"{DateTime.Now} FATAL ERROR in {context}: {ex.Message}\n{ex}\n\n");
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var error = $"{DateTime.Now} UNHANDLED EXCEPTION: {e.Message}\nDETAILS: {e.Exception}";
        Debug.WriteLine(error);
        Trace.WriteLine(error);
        LogFatalError("UnhandledException", e.Exception);
        e.Handled = true;       

        if (Host != null)
        {
            try
            {
                await GetService<IDialogService>().ShowMessageAsync("Lỗi hệ thống", $"Đã xảy ra lỗi không mong muốn: {e.Message}");
            }
            catch { }
        }
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {                                    
            base.OnLaunched(args);
            //ApplicationLanguages.PrimaryLanguageOverride = "vi-VN";
            //var culture = new System.Globalization.CultureInfo("vi-VN");
            //System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            //System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            Debug.WriteLine("Activating services...");            
            splash_screen = new SplashScreen();            
            splash_screen.Activate();
            await splash_screen.SetWindowPositionToCenter();

            MainWindow = new MainWindow();

            // ActivationService will set MainWindow.Content and call MainWindow.Activate()
            await App.GetService<IActivationService>().ActivateAsync(args);
            
            // Keep splash screen for a minimum time to ensure smooth transition
            await Task.Delay(2000);
            
            MainWindow.CenterOnScreen();
            splash_screen.Close();

            Debug.WriteLine("Services activated.");

            _ = Task.Run(async () =>
            {
                await Task.Delay(2000); // Wait a bit after UI is ready
                await App.GetService<IUpdateService>().CheckForUpdatesAsync();
            });
            
        }
        catch (Exception ex)
        {
            var error = $"LAUNCH ERROR: {ex.Message}\nSTACKTRACE: {ex.StackTrace}";
            Debug.WriteLine(error);
            Trace.WriteLine(error);
            await GetService<IDialogService>().ShowMessageAsync("Lỗi khởi động", $"Có lỗi xảy ra khi khởi động ứng dụng: {ex.Message}");
        }
    }
  
    

}
