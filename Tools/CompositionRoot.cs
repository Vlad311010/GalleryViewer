using App.PreviewCreation;
using App.Services;
using App.Settings;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Tools.Sync;

namespace Tools
{
    internal class CompositionRoot : IDisposable
    {
        private readonly ILoggerFactory loggerFactory;

        public PreviewCreationService PreviewCreator { get; }
        public GalleriesService Galleries { get; }
        public AssetsService Images { get; }
        public GroupsService Groups { get; }
        public FileSystemMediaAccessorService MediaAccessorService { get; }
        public PersistenceService Persistence { get; }

        public CompositionRoot(AssetsCatalogContext context, PreviewSettings previewSettings)
        {
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(Log.Logger);
            });

            MediaAccessorService = new FileSystemMediaAccessorService();
            PreviewCreator = new PreviewCreationService(Options.Create(previewSettings), loggerFactory.CreateLogger<PreviewCreationService>());
            Galleries = new GalleriesService(context, loggerFactory.CreateLogger<GalleriesService>());
            Images = new AssetsService(context, MediaAccessorService, loggerFactory.CreateLogger<AssetsService>());
            Groups = new GroupsService(context, loggerFactory.CreateLogger<GroupsService>());
            Persistence = new PersistenceService(context);
        }

        public GallerySync CreateGallerySync()
        {
            return new GallerySync(
                Galleries,
                Images,
                PreviewCreator,
                Groups,
                Persistence,
                loggerFactory.CreateLogger<GallerySync>()
            );
        }

        public void Dispose()
        {
            loggerFactory.Dispose();
        }
    }
}
