using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("products")]
public class Products : BaseModel
{    
    [PrimaryKey("product_id", false)]
    public long ProductID { get; set;}
    [Column("name")]
    public string Name { get; set;}
    [Column("base_price")]
    public double BasePrice { get; set;}
    [Column("price_odd")]
    public int PriceOdd { get; set;}
    [Column("price_even")]
    public int PriceEven { get; set;}  
    [Column("inventory")]
    public int Inventory { get; set;}   
    [Column("pr_wage")]
    public double PrWage { get; set;}

    // --- Coefficients (Exact naming from SQL) ---
    [Column("s_kieng")]
    public double sKieng { get; set;}
    [Column("s_nhl")]
    public double sNhL { get; set;}
    [Column("s_nhn")]
    public double sNhN { get; set;}
    [Column("s_gl")]
    public double sG_l { get; set;}
    [Column("s_gn")]
    public double sG_n { get; set;}
    [Column("s_dl")]
    public double sDl { get; set;}
    [Column("s_hau")]
    public double sHau { get; set;}
    [Column("s_lua")]
    public double sLua { get; set;}
    [Column("s_kt")]
    public double sKt { get; set;}
    [Column("s_oc")]
    public double sOc { get; set;}
    [Column("s_nhom")]
    public double sNhom { get; set;}
    [Column("s_7f")]
    public double s7f { get; set;}
    [Column("s_2d")]
    public double s2D { get; set;}
    [Column("s_decal")]
    public double sDecal { get; set;}   
    [Column("mdf_odd")]
    public double mdfOdd { get; set;}    
    [Column("mdf_even")]
    public double mdfEven { get; set;}    
    [Column("hp_odd")]
    public double hpOdd { get; set;}    
    [Column("hp_even")]
    public double hpEven { get; set;}  
    [Column("hoanh")]
    public double hoanh { get; set;}  
    [Column("lieng")]
    public double lieng { get; set;}    
    [Column("tg")]
    public double tg { get; set; }           

    [Column("size_id")]
    public string SizeID { get; set; }
    }
