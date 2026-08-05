using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

public partial class AssetsCatalogContext : DbContext
{
    public AssetsCatalogContext()
    {
    }

    public AssetsCatalogContext(DbContextOptions<AssetsCatalogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetGroup> AssetGroups { get; set; }

    public virtual DbSet<AssetTag> AssetTags { get; set; }

    public virtual DbSet<Gallery> Galleries { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagCategory> TagCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=F:\\_saves\\DB\\GalleryViewerDataTest.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");

            entity.HasIndex(e => e.Hash, "Asset_Hash_IDX");

            entity.HasOne(d => d.Gallery).WithMany(p => p.Assets).HasForeignKey(d => d.GalleryId);

            entity.HasOne(d => d.Group).WithMany(p => p.Assets).HasForeignKey(d => d.GroupId);
        });

        modelBuilder.Entity<AssetTag>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => new { e.AssetId, e.TagId }, "ImageTags_ImageId_IDX").IsUnique();

            entity.HasIndex(e => new { e.TagId, e.AssetId }, "ImageTags_TagId_IDX").IsUnique();

            entity.HasOne(d => d.Asset).WithMany().HasForeignKey(d => d.AssetId);

            entity.HasOne(d => d.Tag).WithMany().HasForeignKey(d => d.TagId);
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.HasOne(d => d.CoverSource).WithMany(p => p.Galleries).HasForeignKey(d => d.CoverSourceId);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.CanonicalId, "Tags_CanonicalId_IDX");

            entity.HasIndex(e => e.CategoryId, "Tags_CategoryId_IDX");

            entity.HasIndex(e => e.Name, "Tags_Name_IDX").IsUnique();

            entity.HasOne(d => d.Canonical).WithMany(p => p.InverseCanonical)
                .HasForeignKey(d => d.CanonicalId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.Tags)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
