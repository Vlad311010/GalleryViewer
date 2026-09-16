using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public partial class AssetsCatalogContext
    {
#pragma warning disable CA1822 // Mark members as static
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
#pragma warning restore CA1822 // Mark members as static
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssetsCatalogContext).Assembly);
        }
    }
}
