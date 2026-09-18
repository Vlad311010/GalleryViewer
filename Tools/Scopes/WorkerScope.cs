using App.Interfaces.Services;
using App.PreviewCreation;
using App.Services;
using Data.Context;

namespace Tools.Scopes
{
    internal sealed class WorkerScope : DbContextScope<AssetsCatalogContext>
    {
        public IAssetsService Assets { get; }
        public IGroupsService Groups { get; }
        public IGalleriesService Galleries { get; }
        public IPreviewCreationService Preview { get; }
        public IPersistenceService Persistence { get; }

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
