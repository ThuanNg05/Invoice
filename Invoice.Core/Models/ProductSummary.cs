using CommunityToolkit.Mvvm.ComponentModel;

namespace Invoice.Core.Models;

public partial class ProductSummary : ObservableObject
{
    [ObservableProperty]
    private long productID;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private double basePrice;

    [ObservableProperty]
    private int priceOdd;

    [ObservableProperty]
    private int priceEven;

    [ObservableProperty]
    private int inventory;

    [ObservableProperty]
    private int originalInventory;
}
