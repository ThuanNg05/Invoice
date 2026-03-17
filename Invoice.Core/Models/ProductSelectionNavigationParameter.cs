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
    public Func<long, bool> CheckProductExists
    {
        get; set;
    }
    public Action<long, int> OnIncreaseAmount
    {
        get; set;
    }
}
