using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("listplanks")]
public class DetailPlanks : BaseModel
{
    [PrimaryKey("size_id", true)]
    [Column("size_id")]
    public string sizeID { get; set; }

    [Column("inventory")]
    public int inventory
    {
        get; set;
    }

    [Column("status")]
    public int Status { get; set; } = 1;
    }