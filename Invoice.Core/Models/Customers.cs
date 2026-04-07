using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("customers")]
public class Customers : BaseModel
{
    [PrimaryKey("customer_id")]
    public long CustomerID
    {
        get; set;
    }

    [Column("name")]
    public string Name
    {
        get; set;
    }

    [Column("phone")]
    public string Phone
    {
        get; set;
    }

    [Column("price_group")]
    public string PriceGroup
    {
        get; set;
    }

    [Column("status")]
    public int Status { get; set; } = 1;

    public override string ToString()
    {
        return Name;
    }
}