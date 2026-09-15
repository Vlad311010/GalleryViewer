using App.Commands;
using App.Enums;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Gallery;
using App.Models.Dtos.Group;
using App.Models.Queries;
using App.PreviewCreation;
using App.Services;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using Spectre.Console;
using System.Threading.Channels;
using Tools.Models;
using Tools.Scopes;
using Tools.Utils;

namespace Tools.Sync
{
    internal class GallerySync : IDisposable
    {
        private readonly CompositionRoot root;

        private readonly GallerySyncScope syncScope;
        private readonly ILogger<GallerySync> logger;


        private readonly GallerySyncData syncData;
        private readonly SyncStateTracker stateTracker;

        private const int WorkersCount = 4;
        private readonly object progressLock = new();

        private bool isDisposed;


        public GallerySync(GallerySyncData syncData, CompositionRoot root, ILogger<GallerySync> logger)
        {
            ArgumentNullException.ThrowIfNull(syncData);
            ArgumentNullException.ThrowIfNull(root);
            ArgumentNullException.ThrowIfNull(logger);

            syncScope = root.ConstructGallerySyncScope();

            this.root = root;
            this.logger = logger;
            this.syncData = syncData;

            stateTracker = new(syncData.Galeries.Select(x => x.Name));
        }

        public event EventHandler<SyncState>? OnProgressUpdated;

        private void ProgressUpdate(SyncEvent syncEvent)
        {
            SyncState state;
            lock (progressLock)
            {
                state = this.stateTracker.Apply(syncEvent);
            }

            OnProgressUpdated?.Invoke(this, state);
        }

        public async Task Syncronize()
        {
            logger.Info(
                "Starting gallery synchronization for {GalleryCount} configured galleries", ApplicationArea.Tools,
                syncData.Galeries.Count()
            );

            foreach (var galleryConfigData in syncData.Galeries)
            {
                (IEnumerable<FilesGroup> files, int filesCount) = FileDiscovery.Discover(galleryConfigData.Path);
                if (filesCount == 0)
                {
                    logger.Warning(
                        "Skipping gallery {GalleryName}: no supported files found at {GalleryPath}", ApplicationArea.Tools,
                        galleryConfigData.Name,
                        galleryConfigData.Path
                    );

                }
                stateTracker.UpdateGalleriesFilesCount(galleryConfigData.Name, filesCount);
                ProgressUpdate(new SyncEvent(
                    SyncEventType.GalleryProcessingStarted,
                    galleryConfigData.Name,
                    null)
                );

                bool isNewGallery = false;
                GalleryDto? gallery = await syncScope.Galleries.GetByNameAsync(new(galleryConfigData.Name));
                if (gallery == null)
                {
                    isNewGallery = true;
                    GalleryCreateCommand galleryCreate = new GalleryCreateCommand(galleryConfigData.Name, galleryConfigData.Path);
                    gallery = await syncScope.Galleries.Create(galleryCreate);

                    ProgressUpdate(new SyncEvent(
                        SyncEventType.GalleryCreated,
                        gallery.Name,
                        null)
                    );
                }
                else if (!IsMatchesWithConfig(gallery, galleryConfigData))
                {
                    logger.Warning(
                        "Skipping gallery synchronization because configuration does not match existing gallery. " +
                        "GalleryId={GalleryId}, ExistingName={ExistingName}, ConfiguredName={ConfiguredName}, " +
                        "ExistingPath={ExistingPath}, ConfiguredPath={ConfiguredPath}", ApplicationArea.Tools,
                        gallery.Id,
                        gallery.Name,
                        galleryConfigData.Name,
                        gallery.Path,
                        galleryConfigData.Path
                    );

                    continue;
                }

                ProgressUpdate(new SyncEvent(
                    SyncEventType.GalleryProcessingStarted,
                    gallery.Name,
                    null)
                );
                logger.Info("Synchronizing gallery {GalleryName} Id:{id}", ApplicationArea.Tools,
                    galleryConfigData.Name,
                    gallery.Id
                );

                await SyncGalleryAsync(gallery, files);

                ProgressUpdate(new SyncEvent(
                    SyncEventType.GalleryProcessingFinished,
                    gallery.Name,
                    null)
                );

                // gallery thumbnail
                string? thumbnailFile = galleryConfigData.ThumbnailFile;
                if (isNewGallery)
                {
                    thumbnailFile ??= files.First().Files.First();
                }

                UpdateGalleryThumbnailAsync(gallery.Id, gallery.CoverAssetId, thumbnailFile);

                logger.Info(
                    "Gallery synchronization completed for {GalleryName}. " +
                    "Files={TotalFilesToProcess}, " +
                    "CreatedAssets={CreatedAssets}, " +
                    "SkippedAssets={SkippedAssets}, " +
                    "DeletedAssets={DeletedAssets}, " +
                    "CreatedGroups={CreatedGroups}", ApplicationArea.Tools,
                    stateTracker[gallery.Name].GalleryName,
                    stateTracker[gallery.Name].TotalFilesToProcess!,
                    stateTracker[gallery.Name].CreatedAssets,
                    stateTracker[gallery.Name].SkippedAssets,
                    stateTracker[gallery.Name].DeletedAssets,
                    stateTracker[gallery.Name].CreatedGroups);
            }
        }

        private async void UpdateGalleryThumbnailAsync(int galleryId, int? currentThumbnailAssetId, string? thumbnailFile)
        {
            if (thumbnailFile != null)
            {
                var thumbnailAsset = await syncScope.Assets.GetByPathAsync(galleryId, thumbnailFile);
                if (thumbnailAsset == null)
                {
                    logger.Warning(
                        "Configured thumbnail asset not found. GalleryId={GalleryId}, AssetPath={AssetPath}", ApplicationArea.Tools,
                        galleryId,
                        thumbnailFile
                    );
                    return;
                }

                if (currentThumbnailAssetId.HasValue && thumbnailAsset.Id != currentThumbnailAssetId.Value)
                {
                    await syncScope.Galleries.StageUpdatePreviewAssetAsync(galleryId, thumbnailAsset.Id);
                }
            }
        }

        private async Task SyncGalleryAsync(GalleryDto gallery, IEnumerable<FilesGroup> filesGroups)
        {
            var channel = Channel.CreateUnbounded<AssetSyncData>(
                new UnboundedChannelOptions
                {
                    SingleWriter = true,
                    SingleReader = false
                }
            );

            foreach (var filesGroup in filesGroups)
            {
                if (string.IsNullOrEmpty(filesGroup.Folder)) // no group --- root folder
                {
                    await QueueAssetsAsync(gallery, null, filesGroup.Files, channel.Writer);
                }
                else
                {
                    AssetGroupDto? group = await syncScope.Groups.GetPhysicalGroup(new PhysicalAssetGroupQuery(gallery.Id, filesGroup.Folder));
                    if (group == null)
                    {
                        group = await CreateGroupAsync(gallery, filesGroup);
                        await QueueAssetsAsync(gallery, group.Id, filesGroup.Files, channel.Writer);

                    }
                    else if (!syncScope.Groups.IsSynchronized(new AssetGroupSynchronizationQuery(group.Id, filesGroup.Files), out List<AssetSynchronizationDto> outOfSyncAsset)) // out of sync
                    {
                        AssetSyncData[] assetsToProcess = await SyncGroupAsync(gallery, group, outOfSyncAsset);
                        await QueueAssetsAsync(assetsToProcess, channel.Writer);
                    }
                    else // fine and up to date 
                    {
                        ProgressUpdate(new SyncEvent(
                            SyncEventType.GroupSyncSkipped,
                            gallery.Name,
                            null,
                            filesGroup.Files.Length)
                        );

                        logger.Debug(
                            "Group {GroupId} is already synchronized", ApplicationArea.Tools,
                            group.Id
                        );
                    }
                }
            }

            await syncScope.Persistence.SaveChangesAsync();
            channel.Writer.Complete();

            var workers = Enumerable.Range(0, WorkersCount)
                .Select(idx => RunAssetWorkerAsync(channel.Reader, idx))
                .ToArray();


            await Task.WhenAll(workers);
            await syncScope.Persistence.SaveChangesAsync();
        }

        private static async Task QueueAssetsAsync(GalleryDto gallery, int? groupId, string[] files, ChannelWriter<AssetSyncData> channelWriter)
        {
            for (int position = 0; position < files.Length; position++)
            {
                await channelWriter.WriteAsync(new AssetSyncData(
                    gallery.Name,
                    gallery.Id,
                    groupId,
                    gallery.Path,
                    files[position],
                    groupId.HasValue ? position : 0));
            }
        }

        private static async Task QueueAssetsAsync(IEnumerable<AssetSyncData> assetSyncData, ChannelWriter<AssetSyncData> channelWriter)
        {
            foreach (var assetData in assetSyncData)
            {
                await channelWriter.WriteAsync(assetData);
            }
        }

        private async Task<AssetGroupDto> CreateGroupAsync(GalleryDto gallery, FilesGroup filesGroup)
        {
            DateTime creationTime = Directory.GetCreationTimeUtc(
                Path.Combine(gallery.Path, filesGroup.Folder));

            CreateAssetGroupCommand createDto = new(gallery.Id, filesGroup.Folder, filesGroup.Folder, creationTime);
            AssetGroupDto group = await syncScope.Groups.CreateGroup(createDto);

            ProgressUpdate(new SyncEvent(
                SyncEventType.GroupCreated,
                gallery.Name,
                null)
            );

            return group;
        }

        private async Task<AssetSyncData[]> SyncGroupAsync(GalleryDto gallery, AssetGroupDto group, List<AssetSynchronizationDto> outOfSyncAsset)
        {
            List<int> assetIdsToDelete = new List<int>();
            List<string> missingAssetPaths = new List<string>();

            foreach (var asset in outOfSyncAsset)
            {
                if (asset.Type == SyncMismatchType.OnlyDb) // files were removed from file system
                {
                    assetIdsToDelete.Add(asset.id!.Value);
                }
                else if (asset.Type == SyncMismatchType.OnlyFileSystem) // new files added inside folder in file system
                {
                    missingAssetPaths.Add(asset.RelativePath);
                }
            }

            logger.Info(
                "Group {GroupId} synchronization detected changes: {AssetsToRemove} assets to remove, {AssetsToAdd} assets to add", ApplicationArea.Tools,
                group.Id,
                assetIdsToDelete.Count,
                missingAssetPaths.Count
            );

            // delete
            int deleted = await syncScope.Assets.DeleteRangeAsync(assetIdsToDelete);

            ProgressUpdate(new SyncEvent(
                SyncEventType.AssetDeleted,
                gallery.Name,
                null,
                updateValue: deleted)
            );

            // normalize group positions
            int groupPositionOffset;
            if (deleted > 0)
            {
                groupPositionOffset = 1 + await syncScope.Groups.StageNormalizePositionsAsync(new(group!.Id));
            }
            else
            {
                groupPositionOffset = await syncScope.Groups.AssetsCountAsync(new(group!.Id));
            }

            return missingAssetPaths
               .Select((file, i) => new AssetSyncData
               (
                   gallery.Name,
                   gallery.Id,
                   group.Id,
                   gallery.Path,
                   file,
                   i + groupPositionOffset
               ))
               .ToArray();
        }

        private async Task RunAssetWorkerAsync(ChannelReader<AssetSyncData> reader, int workerId)
        {
            using var scope = root.ConstructWorkerScope();

            await foreach (var assetSyncData in reader.ReadAllAsync())
            {
                await ProcessAssetAsync(
                    assetSyncData,
                    scope.Assets,
                    scope.Preview,
                    scope.Persistence
                );
            }

            await scope.Persistence.SaveChangesAsync();
        }

        private async Task ProcessAssetAsync(AssetSyncData assetSyncData, AssetsService assetsService, PreviewCreationService previewCreator, PersistenceService persistenceService)
        {
            if (await assetsService.ExistsAsync(assetSyncData.GalleryId, assetSyncData.AssetRelativePath))
            {
                logger.Debug(
                    "Skipping existing asset {AssetPath} in gallery {GalleryId}", ApplicationArea.Tools,
                    assetSyncData.AssetRelativePath,
                    assetSyncData.GalleryId
                );

                ProgressUpdate(new SyncEvent(
                    SyncEventType.AssetSkipped,
                    assetSyncData.GalleryName,
                    assetSyncData.AssetRelativePath)
                );

                return;
            }

            var previewPath = await previewCreator.CreatePreviewAsync(assetSyncData.GalleryPath, assetSyncData.AssetRelativePath);
            await assetsService.StageCreateAssetAsync(
                new AssetCreateCommand
                (
                    assetSyncData.GalleryId,
                    assetSyncData.AssetRelativePath,
                    previewPath,
                    assetSyncData.GroupId,
                    assetSyncData.Position
                )
            );

            ProgressUpdate(new SyncEvent(
                SyncEventType.AssetCreated,
                assetSyncData.GalleryName,
                assetSyncData.AssetRelativePath)
            );
        }


        private static bool IsMatchesWithConfig(GalleryDto gallery, GaleryData galleryConfigData)
        {
            return string.Equals(gallery.Name, galleryConfigData.Name, StringComparison.OrdinalIgnoreCase) && gallery.Path == galleryConfigData.Path;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (disposing)
                {
                    syncScope.Dispose();
                }
            }

            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private record AssetSyncData(
            string GalleryName,
            int GalleryId,
            int? GroupId,
            string GalleryPath,
            string AssetRelativePath,
            int Position
        );
    }
}
