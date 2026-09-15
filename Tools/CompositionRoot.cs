using App.PreviewCreation;
using App.Services;
using App.Settings;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Tools.Models;
using Tools.Scopes;
using Tools.Sync;

namespace Tools
{
    internal class CompositionRoot : IDisposable
    {
        private bool disposed = false;
        private readonly ILoggerFactory loggerFactory;
        private readonly DbContextOptions<AssetsCatalogContext> contextOptions;

        public readonly PreviewCreationService previewCreator;
        public PreviewCreationService PreviewCreator => previewCreator;

        public readonly FileSystemMediaAccessorService mediaAccessorService;
        public FileSystemMediaAccessorService MediaAccessorService => mediaAccessorService;

        public CompositionRoot(DbContextOptions<AssetsCatalogContext> contextOptions, PreviewSettings previewSettings)
        {
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(Log.Logger);
            });
            this.contextOptions = contextOptions;

            previewCreator = new PreviewCreationService(Options.Create(previewSettings), loggerFactory.CreateLogger<PreviewCreationService>());
            mediaAccessorService = new FileSystemMediaAccessorService();
        }

        public AssetsCatalogContext ConstructDbContext()
        {
            return new AssetsCatalogContext(contextOptions);
        }

        public GalleriesService ConstructGalleriesService(AssetsCatalogContext context)
        {
            return new GalleriesService(context, loggerFactory.CreateLogger<GalleriesService>());
        }

        public AssetsService ConstructAssetsService(AssetsCatalogContext context)
        {
            return new AssetsService(context, MediaAccessorService, loggerFactory.CreateLogger<AssetsService>());
        }

        public GroupsService ConstructGroupsService(AssetsCatalogContext context)
        {
            return new GroupsService(context, loggerFactory.CreateLogger<GroupsService>());
        }

        public PersistenceService ConstructPersistenceService(AssetsCatalogContext context)
        {
            return new PersistenceService(context);
        }

        public WorkerScope ConstructWorkerScope()
        {
            AssetsCatalogContext context = ConstructDbContext();
            return new WorkerScope(
                context,
                ConstructGalleriesService(context),
                ConstructGroupsService(context),
                ConstructAssetsService(context),
                PreviewCreator,
                ConstructPersistenceService(context)
            );
        }

        public GallerySyncScope ConstructGallerySyncScope()
        {
            AssetsCatalogContext context = ConstructDbContext();
            return new GallerySyncScope(
                context,
                ConstructGalleriesService(context),
                ConstructGroupsService(context),
                ConstructAssetsService(context),
                ConstructPersistenceService(context)
            );
        }


        public GallerySync CreateGallerySync(GallerySyncData syncData)
        {
            return new GallerySync(syncData, this, loggerFactory.CreateLogger<GallerySync>());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                loggerFactory.Dispose();
            }

            disposed = true;
        }
    }
}
