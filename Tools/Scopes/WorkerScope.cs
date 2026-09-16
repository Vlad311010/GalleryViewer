using App.PreviewCreation;
using App.Services;
using Data.Context;

namespace Tools.Scopes
{
    internal sealed class WorkerScope : DbContextScope<AssetsCatalogContext>
    {
        public AssetsService Assets { get; }
        public GroupsService Groups { get; }
        public GalleriesService Galleries { get; }
        public PreviewCreationService Preview { get; }
        public PersistenceService Persistence { get; }

        public WorkerScope(
            AssetsCatalogContext context,
            GalleriesService galleries,
            GroupsService groups,
            AssetsService assets,
            PreviewCreationService preview,
            PersistenceService persistence) : base(context)
        {
            Assets = assets;
            Groups = groups;
            Galleries = galleries;
            Preview = preview;
            Persistence = persistence;
        }
    }
}
