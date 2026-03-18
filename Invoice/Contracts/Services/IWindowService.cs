using Invoice.Core.Models;

namespace Invoice.Contracts.Services;

public interface IWindowService
{
    void OpenProductSelectionWindow(Customers selectedCustomer);
    void OpenHistoryWindow(Action<string> onInvoiceSelected);
    void CloseProductSelectionWindow();
    void CloseHistoryWindow();
}
