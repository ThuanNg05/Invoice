namespace Invoice.Core.Models;

public class ProductSummary
{
    public string ProductID { get; set; }
    public string Name { get; set; }
    public double BasePrice { get; set; }
    public int PriceOdd { get; set; }
    public int PriceEven { get; set; }
    public int Inventory { get; set; }
}
