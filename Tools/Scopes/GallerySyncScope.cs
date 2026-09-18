using App.Interfaces.Services;
using App.Services;
using Data.Context;

namespace Tools.Scopes
{
    internal sealed class GallerySyncScope : DbContextScope<AssetsCatalogContext>
    {
        public IGalleriesService Galleries { get; }
        public IGroupsService Groups { get; }
        public IAssetsService Assets { get; }
        public IMediaAccessorService MediaAccessor { get; }
        public IPersistenceService Persistence { get; }

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
