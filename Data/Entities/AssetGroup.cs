using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class AssetGroup
{
    public int Id { get; set; }

    public int CoverAssetIdx { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
