using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("invoice_details")]
public class InvoiceDetail : BaseModel
{
    [PrimaryKey("invoice_id", true)]
    [JsonProperty("invoice_id")]
    public string InvoiceID { get; set; }

    [PrimaryKey("product_id", true)]
    [JsonProperty("product_id")]
    public long ProductID { get; set; }    

    [Column("product_name")]
    [JsonProperty("product_name")]
    public string ProductName { get; set; }   

    [Column("sell_price")]
    [JsonProperty("sell_price")]
    public int SellPrice { get; set; }  

    [Column("amount")]
    [JsonProperty("amount")]
    public int Amount { get; set; }

    [Column("note")]
    [JsonProperty("note")]
    public string Note { get; set; }

    [Column("line_total", ignoreOnInsert: true, ignoreOnUpdate: true)]
    [JsonProperty("line_total")]
    public int? LineTotal { get; set; }

    [Column("customer")]
    [JsonProperty("customer")]
    public string CustomerName { get; set; }      

    [Reference(typeof(Invoices))]
    [JsonProperty("Invoice")]
    public Invoices Invoice { get; set; }
}
