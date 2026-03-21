using Invoice.Core.Models;

namespace Invoice.Core.Models;

public class ProductSelectionNavigationParameter
{
    public string PriceGroup { get; set; }
    public IEnumerable<TempInvoice> CurrentInvoiceItems { get; set; } = [];
}
