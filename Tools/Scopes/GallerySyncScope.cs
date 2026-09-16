using App.Services;
using Data.Context;

namespace Tools.Scopes
{
    internal sealed class GallerySyncScope : DbContextScope<AssetsCatalogContext>
    {
        public AssetsService Assets { get; }
        public GroupsService Groups { get; }
        public GalleriesService Galleries { get; }
        public PersistenceService Persistence { get; }

        public GallerySyncScope(
            AssetsCatalogContext context,
            GalleriesService galleries,
            GroupsService groups,
            AssetsService assets,
            PersistenceService persistence) : base(context)
        {
            Assets = assets;
            Groups = groups;
            Galleries = galleries;
            Persistence = persistence;
        }
    }
}
