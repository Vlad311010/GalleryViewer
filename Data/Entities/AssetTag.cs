using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class AssetTag
{
    public int AssetId { get; set; }

    public int TagId { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
