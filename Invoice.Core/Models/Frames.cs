using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Invoice.Core.Models;

[Table("planks")]
public class Frames : BaseModel
{
    [PrimaryKey("frame_id", false)]
    public int? FrameID
    {
        get; set;
    }

    [Column("frame_no")]
    public string FrameNO
    {
        get; set;
    }

    [Column("size1")]
    public string size1
    {
        get; set;
    }

    [Column("size2")]
    public string size2
    {
        get; set;
    }

    [Column("size3")]
    public string size3
    {
        get; set;
    }

    [Column("size4")]
    public string size4
    {
        get; set;
    }

    [Column("size5")]
    public string size5
    {
        get; set;
    }

    [Column("size6")]
    public string size6
    {
        get; set;
    }

    [Column("size7")]
    public string size7
    {
        get; set;
    }

    [Column("size8")]
    public string size8
    {
        get; set;
    }

    [Column("size9")]
    public string size9
    {
        get; set;
    }

    [Column("size10")]
    public string size10
    {
        get; set;
    }

    [Column("description")]
    public string Description
    {
        get; set;
    }
}