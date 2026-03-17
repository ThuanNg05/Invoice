using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("materials")]
public class Materials : BaseModel
{
    [PrimaryKey("product_id", false)]
    public long ProductID { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("base_price")]
    public decimal BasePrice { get; set; }    
    
    [Column("inventory")]
    public int Inventory { get; set; }        
    
    [Column("total_line", ignoreOnInsert: true, ignoreOnUpdate: true)] 
    public decimal TotalLine { get; set; }      

    [Column("min_amount")]
    public int MinAmount { get; set; }        
}
