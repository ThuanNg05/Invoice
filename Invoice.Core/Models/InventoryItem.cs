using CommunityToolkit.Mvvm.ComponentModel;

namespace Invoice.Core.Models;

public partial class InventoryItem : ObservableObject
{
    [ObservableProperty]
    private string productID;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private int inventory;

    [ObservableProperty]
    private string source;
}