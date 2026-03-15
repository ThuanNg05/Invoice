using Invoice.Core.Models;

namespace Invoice.Core.Contracts;

// Donot delete this class, it is used as a message token only
public enum DataAction
{
    Create, Update, Delete
}
public class ProductsChangedMessage
{
    //public Products Product { get; set; }
    //public int Amount { get; set; }
    //public int FinalPrice { get; set; }
    //public string Note { get; set; }
    //public bool IsMerge { get; set; }

    public DataAction Action
    {
        get;
    }
    public Products Product
    {
        get;
    }
    public string ProductId
    {
        get;
    }

    public ProductsChangedMessage(DataAction action, Products product)
    {
        Action = action;
        Product = product;
        ProductId = product?.ProductID;
    }

    // Constructor dùng cho trường hợp Delete chỉ có ID
    public ProductsChangedMessage(DataAction action, string productId)
    {
        Action = action;
        ProductId = productId;
    }
}
