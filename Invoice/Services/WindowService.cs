using Invoice.Contracts.Services;
using Invoice.Core.Models;
using Invoice.Helpers;
using Invoice.Views;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Services;

public class WindowService : IWindowService
{
    private WindowEx? _productSelectionWindow;
    private WindowEx? _editingWindow;

    public void OpenProductSelectionWindow(Customers selectedCustomer, IEnumerable<TempInvoice> currentInvoiceItems)
    {
        if (_productSelectionWindow != null)
        {
            _productSelectionWindow.Activate();
            return;
        }

        var newWindow = new WindowEx
        {
            Title = "Chọn sản phẩm",
            Height = 800,
            Width = 1200
        };
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
            PriceGroup = selectedCustomer.PriceGroup,
            CurrentInvoiceItems = currentInvoiceItems
        };

        frame.Navigate(typeof(ProductSelectionPage), navParam);
        newWindow.Activate();
    }

    public void OpenEditingInvoiceWindow(Action<string> onInvoiceSelected)
    {
        if (_editingWindow != null)
        {
            _editingWindow.Activate();
            return;
        }

        var newWindow = new WindowEx
        {
            Title = "Sửa hoá đơn",
            Height = 800,
            Width = 1200
        };
        newWindow.CenterOnScreen();

        _editingWindow = newWindow;
        newWindow.Closed += (sender, args) =>
        {
            _editingWindow = null;
        };

        var frame = new Frame();
        newWindow.Content = frame;

        var navParam = new EditingInvoiceNavigationParameter
        {
            OnInvoiceSelected = (invoiceId) =>
            {
                onInvoiceSelected?.Invoke(invoiceId);
                CloseEditingWindow();
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

    public void CloseEditingWindow()
    {
        _editingWindow?.Close();
        _editingWindow = null;
    }
}
