using Invoice.Core.Models;

namespace Invoice.Contracts.Services;

public interface IWindowService
{
    void OpenProductSelectionWindow(Customers selectedCustomer, IEnumerable<TempInvoice> currentInvoiceItems);
    void OpenEditingInvoiceWindow(Action<string> onInvoiceSelected);
    void CloseProductSelectionWindow();
    void CloseEditingWindow();
}
