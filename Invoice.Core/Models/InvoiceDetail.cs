using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("invoice_details")]
public class InvoiceDetail : BaseModel
{
    [PrimaryKey("invoice_id", true)]
    public string InvoiceID { get; set; }
    [PrimaryKey("product_id", true)]
    public long ProductID { get; set; }    
    [Column("product_name")]
    public string ProductName { get; set; }   
    [Column("sell_price")]
    public int SellPrice { get; set; }  
    [Column("amount")]
    public int Amount { get; set; }
    [Column("note")]
    public string Note { get; set; }
    [Column("line_total", ignoreOnInsert: true, ignoreOnUpdate: true)]
    public int? LineTotal { get; set; }
    [Column("customer")]
    public string CustomerName { get; set; }      

    [Reference(typeof(Invoices))]
    public Invoices Invoice { get; set; }
}
