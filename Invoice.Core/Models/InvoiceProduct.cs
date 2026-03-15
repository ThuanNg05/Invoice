using CommunityToolkit.Mvvm.ComponentModel;

namespace Invoice.Core.Models;

public partial class InvoiceProduct : ObservableObject
{
    [ObservableProperty]
    private string proCode;
    
    [ObservableProperty]
    private string proName;
   
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProTotal))] 
    private decimal proPrice;
   
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProTotal))]
    private int proQuantity;

    [ObservableProperty]
    private string proNote;

    public decimal ProTotal => ProPrice * ProQuantity;
}
