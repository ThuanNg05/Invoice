using CommunityToolkit.Mvvm.ComponentModel;

using Invoice.Contracts.Services;
using Invoice.ViewModels;
using Invoice.Views;

using Microsoft.UI.Xaml.Controls;

namespace Invoice.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {        
        Configure<SettingsViewModel, SettingsPage>();
        Configure<CreateInvoiceViewModel, CreateInvoicePage>();
        Configure<CustomersViewModel, CustomersPage>();
        Configure<DetailPlanksViewModel, DetailPlanksPage>();
        Configure<DetailPriceViewModel, DetailPricePage>();
        Configure<EditingInvoiceViewModel, EditingInvoicePage>();
        Configure<HistoryViewModel, HistoryPage>();
        Configure<IOActionsViewModel, IOActionsPage>();
        Configure<MaterialsViewModel, MaterialsPage>();
        Configure<ProductSelectionViewModel, ProductSelectionPage>();
        Configure<ProductsViewModel, ProductsPage>();
        Configure<ReportingViewModel, ReportingPage>();
        Configure<IOPlanksViewModel, IOPlanksPage>();
        Configure<PlanksViewModel, PlanksPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
