using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("invoices")]
public class Invoices : BaseModel
{
    [PrimaryKey("invoice_id", true)]
    [JsonProperty("invoice_id")]
    public string InvoiceID { get; set; }

    [Column("customer_id")]
    [JsonProperty("customer_id")]
    public int? CustomerID { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    public string CreatedDate { get; set; }

    [Column("total")]
    [JsonProperty("total")]
    public int Total { get; set; }

    [Reference(typeof(Customers))]
    [JsonProperty("customers")]
    public Customers Customer { get; set; }
}
