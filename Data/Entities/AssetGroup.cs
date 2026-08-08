namespace Data.Entities;

public partial class AssetGroup
{
    public int Id { get; set; }

    public int CoverAssetIdx { get; set; }

    public string? Title { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime ImportTime { get; set; }

    public string? PhysicalRelativePath { get; set; }

    public int GalleryId { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual Gallery Gallery { get; set; } = null!;
}
