namespace Invoice.Core.Models;

public class WarehouseHistoryItem
{
    public DateTime Date { get; set; }
    public string TransactionType { get; set; } // Nhập kho / Xuất kho
    public string ProductName { get; set; }
    public int Amount { get; set; }
    public string Note { get; set; }
    public string SourceType { get; set; } // PRODUCT / MATERIAL
}
