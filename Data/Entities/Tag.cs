using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? CanonicalId { get; set; }

    public virtual Tag? Canonical { get; set; }

    public virtual TagCategory Category { get; set; } = null!;

    public virtual ICollection<Tag> InverseCanonical { get; set; } = new List<Tag>();
}
