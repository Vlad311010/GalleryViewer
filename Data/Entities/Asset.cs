namespace Data.Entities;

public partial class Asset
{
    public int Id { get; set; }

    public string RelativePath { get; set; } = null!;

    public string? PreviewPath { get; set; }

    public int? GroupId { get; set; }

    public int? GroupPosition { get; set; }

    public int GalleryId { get; set; }

    public string MimeType { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public virtual ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();

    public virtual Gallery Gallery { get; set; } = null!;

    public virtual AssetGroup? Group { get; set; }
}
