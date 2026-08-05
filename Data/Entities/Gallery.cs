using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Gallery
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Path { get; set; } = null!;

    public int? CoverSourceId { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual Asset? CoverSource { get; set; }
}
