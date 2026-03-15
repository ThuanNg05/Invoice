using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("detailprices")]
public class DetailPrice : BaseModel
{
    [PrimaryKey("config_id")]
    public int ConfigID { get; set; }

    [Column("pr_kieng")]
    public double PrKieng { get; set; }

    [Column("pr_nhl")]
    public double PrNhL { get; set; }

    [Column("pr_nhn")]
    public double PrNhN { get; set; }

    [Column("pr_gl")]
    public double PrG_l { get; set; }

    [Column("pr_gn")]
    public double PrG_n { get; set; }

    [Column("pr_dl")]
    public double PrDl { get; set; }

    [Column("pr_hau")]
    public double PrHau { get; set; }

    [Column("pr_lua")]
    public double PrLua { get; set; }

    [Column("pr_kt")]
    public double PrKt { get; set; }

    [Column("pr_oc")]
    public double PrOc { get; set; }

    [Column("pr_nhom")]
    public double PrNhom { get; set; }

    [Column("pr_7f")]
    public double Pr7f { get; set; }

    [Column("pr_2d")]
    public double Pr2D { get; set; }

    [Column("pr_decal")]
    public double PrDecal { get; set; }
}
