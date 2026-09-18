using App.Services;
using Data.Context;

namespace Tools.Scopes
{
    internal sealed class GallerySyncScope : DbContextScope<AssetsCatalogContext>
    {
        public GalleriesService Galleries { get; }
        public GroupsService Groups { get; }
        public AssetsService Assets { get; }
        public FileSystemMediaAccessorService MediaAccessor { get; }
        public PersistenceService Persistence { get; }

        public GallerySyncScope(
            AssetsCatalogContext context,
            GalleriesService galleries,
            GroupsService groups,
            AssetsService assets,
            FileSystemMediaAccessorService mediaAccessor,
            PersistenceService persistence) : base(context)
        {
            Galleries = galleries;
            Groups = groups;
            Assets = assets;
            MediaAccessor = mediaAccessor;
            Persistence = persistence;
        }
    }
}
