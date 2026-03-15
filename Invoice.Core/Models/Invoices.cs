using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("invoices")]
public class Invoices : BaseModel
{
    [PrimaryKey("invoice_id", true)]
    public string InvoiceID { get; set; }

    [Column("customer_id")]
    public int? CustomerID { get; set; }

    [Column("created_date")]
    public string CreatedDate { get; set; }

    [Column("total")]
    public int Total { get; set; }

    [Reference(typeof(Customers))]
    public Customers Customer { get; set; }
}
