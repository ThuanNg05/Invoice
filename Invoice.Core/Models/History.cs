namespace Invoice.Core.Models;

public class History
{
    public string InvoiceID
    {
        get; set;
    }
    public DateTime CreatedDate
    {
        get; set;
    }
    public string CustomerName
    {
        get; set;
    }
    public long ProductID { get; set; }
    public string ProductName
    {
        get; set;
    }
    public int SellPrice
    {
        get; set;
    }
    public int Amount
    {
        get; set;
    }
    public int LineTotal
    {
        get; set;
    }
    public string Note
    {
        get; set;
    }
    public int IsQueryable
    {
        get; set;
    }
}