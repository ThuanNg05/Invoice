using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("inventorytransactions")]
public class WarehouseTransaction : BaseModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [PrimaryKey("transaction_id", false)]
    [JsonProperty("transaction_id")]
    public int Id { get; set; }

    [Column("product_id")]
    [JsonProperty("product_id")]
    public long ProductID { get; set; }

    [Column("invoice_id")]
    [JsonProperty("invoice_id")]
    public string InvoiceID { get; set; }        
    
    [Column("date")]
    [JsonProperty("date")]
    public DateTime CreatedDate { get; set; }

    private int _amount;
    [Column("amount")]    
    [JsonProperty("amount")]
    public int Amount
    {
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged(); // <--- Notifies the UI
                OnPropertyChanged(nameof(FinalChange)); // <--- Also update dependent property
            }
        }
    }

    private string _actionType;
    [Column("type")]    
    [JsonProperty("type")]
    public string ActionType
    {
        get => _actionType;
        set
        {
            if (_actionType != value)
            {
                _actionType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FinalChange));
            }
        }
    }

    [Column("note")]
    [JsonProperty("note")]
    public string Note { get; set; }

    [JsonIgnore]
    public int FinalChange => ActionType == "Nhập kho" ? Amount : -Amount;

    private string _name;
    [JsonIgnore]    
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }


}