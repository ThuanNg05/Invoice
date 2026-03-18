using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.Views;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Services;

public class WindowService : IWindowService
{
    private WindowEx? _productSelectionWindow;
    private WindowEx? _historyWindow;

    public void OpenProductSelectionWindow(Customers selectedCustomer)
    {
        if (_productSelectionWindow != null)
        {
            _productSelectionWindow.Activate();
            return;
        }

        var newWindow = new WindowEx();
        newWindow.Title = "Common_ProductSelection".GetLocalized();
        newWindow.Height = 800;
        newWindow.Width = 1200;
        newWindow.CenterOnScreen();

        _productSelectionWindow = newWindow;
        newWindow.Closed += (sender, args) =>
        {
            _productSelectionWindow = null;
        };

        var frame = new Frame();
        newWindow.Content = frame;

        var navParam = new ProductSelectionNavigationParameter
        {
            PriceGroup = selectedCustomer.PriceGroup
        };

        frame.Navigate(typeof(ProductSelectionPage), navParam);
        newWindow.Activate();
    }

    public void OpenHistoryWindow(Action<string> onInvoiceSelected)
    {
        if (_historyWindow != null)
        {
            _historyWindow.Activate();
            return;
        }

        var newWindow = new WindowEx();
        newWindow.Title = "Shell_History".GetLocalized();
        newWindow.Height = 800;
        newWindow.Width = 1200;
        newWindow.CenterOnScreen();

        _historyWindow = newWindow;
        newWindow.Closed += (sender, args) =>
        {
            _historyWindow = null;
        };

        var frame = new Frame();
        newWindow.Content = frame;

        var navParam = new EditingInvoiceNavigationParameter
        {
            OnInvoiceSelected = (invoiceId) =>
            {
                onInvoiceSelected?.Invoke(invoiceId);
                CloseHistoryWindow();
            }
        };
        
        frame.Navigate(typeof(EditingInvoicePage), navParam);
        newWindow.Activate();
    }

    public void CloseProductSelectionWindow()
    {
        _productSelectionWindow?.Close();
        _productSelectionWindow = null;
    }

    public void CloseHistoryWindow()
    {
        _historyWindow?.Close();
        _historyWindow = null;
    }
}
