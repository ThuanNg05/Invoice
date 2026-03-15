namespace Invoice.Core.Models;

public class ProductSelectionNavigationParameter
{
    public string PriceGroup
    {
        get; set;
    }    
    public Action<TempInvoice> OnProductAdded
    {
        get; set;
    }
    public Func<string, bool> CheckProductExists
    {
        get; set;
    }
    public Action<string, int> OnIncreaseAmount
    {
        get; set;
    }
}
