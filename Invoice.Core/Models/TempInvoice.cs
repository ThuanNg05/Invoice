using CommunityToolkit.Mvvm.ComponentModel;

namespace Invoice.Core.Models;

public partial class TempInvoice : ObservableObject
{
    [ObservableProperty]
    private string productID;

    [ObservableProperty]
    private string productName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int sellPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int amount;

    [ObservableProperty]
    private string note;

    [ObservableProperty]
    private int maxStock;

    public double LineTotal => SellPrice * Amount;
    
    partial void OnAmountChanged(int value)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Đang nhập: {value} | Giới hạn MaxStock: {MaxStock} - OnAmountChanged");
        if (MaxStock <= 0)
        {
            if (value != 0) Amount = 0;
            return;
        }

        if (value > MaxStock)
        {
            Amount = MaxStock;
            return;
        }
        
        if (value <= 0)
        {
            Amount = 1;
        }
    }
}