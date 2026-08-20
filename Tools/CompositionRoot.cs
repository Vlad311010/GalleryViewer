using App;
using App.PreviewCreation;
using App.Settings;
using Data.Entities;
using Microsoft.Extensions.Options;

namespace Tools
{
    internal class CompositionRoot
    {
        public PreviewCreatorService PreviewCreator { get; }
        public GalleriesService Galleries { get; }
        public AssetsService Images { get; }
        public GroupsService Groups { get; }
        public PersistenceService Persistence { get; }

        public CompositionRoot(AssetsCatalogContext context, PreviewSettings previewSettings)
        {
            PreviewCreator = new PreviewCreatorService(Options.Create(previewSettings));
            Galleries = new GalleriesService(context);
            Images = new AssetsService(context);
            Groups = new GroupsService(context);
            Persistence = new PersistenceService(context);
        }

        public GallerySync CreateGallerySync()
        {
            return new GallerySync(
                Galleries,
                Images,
                PreviewCreator,
                Groups,
                Persistence);
        }


    }
}
