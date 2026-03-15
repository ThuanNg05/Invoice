using Invoice.Core.Models;

namespace Invoice.Core.Contracts;

// Donot delete this class, it is used as a message token only
public class ProductsSelectedMessage
{
    public Products Product
    {
        get; set;
    }
    public int Amount
    {
        get; set;
    }
    public int FinalPrice
    {
        get; set;
    }
    public string Note
    {
        get; set;
    }
    public bool IsMerge
    {
        get; set;
    }
}
