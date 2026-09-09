using App.Dto.Asset;
using App.Dto.Gallery;
using App.Dto.Group;
using App.PreviewCreation;
using App.Services;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using Tools.Models;

namespace Tools.Sync
{
    internal class GallerySync(
        GalleriesService galleriesService,
        AssetsService assetsService,
        PreviewCreationService previewCreatorService,
        GroupsService gropusService,
        PersistenceService persistence,
        ILogger<GallerySync> logger)
    {
        private SyncState state = new();
        public event EventHandler<SyncState>? OnProgressUpdated;

        private void ProgressUpdate(SyncEvent syncEvent)
        {
            state.Apply(syncEvent);
            OnProgressUpdated?.Invoke(this, state);
        }

        public async Task Syncronize(GallerySyncData syncData)
        {
            state = new();
            state.SetGalleries(syncData.Galeries.Select(x => x.Name));

            logger.Info(
                "Starting gallery synchronization for {GalleryCount} configured galleries", ApplicationArea.Tools,
                syncData.Galeries.Count()
            );

            foreach (var galleryData in syncData.Galeries)
            {
                (IEnumerable<FilesGroup> files, int filesCount) = GetFiles(galleryData);

                if (filesCount == 0)
                {
                    logger.Warning(
                        "Skipping gallery {GalleryName}: no supported files found at {GalleryPath}", ApplicationArea.Tools,
                        galleryData.Name,
                        galleryData.Path
                    );
                    continue;
                }

                state.TrackGallery(galleryData.Name, filesCount);
                ProgressUpdate(new SyncEvent(
                    SyncEventType.GalleryProcessingStarted,
                    galleryData.Name,
                    null,
                    null)
                );

                bool requiresInitialThumbnail = false;
                GalleryDto? gallery = await galleriesService.GetByNameAsync(galleryData.Name);
                if (gallery == null)
                {
                    requiresInitialThumbnail = true;
                    GalleryDtoCreate galleryCreate = new GalleryDtoCreate(galleryData.Name, galleryData.Path);
                    gallery = await galleriesService.Create(galleryCreate);

                    logger.Info(
                        "Synchronizing new gallery {GalleryName}", ApplicationArea.Tools,
                        galleryData.Name
                    );

                    ProgressUpdate(new SyncEvent(
                        SyncEventType.GalleryCreated,
                        gallery.Name,
                        null,
                        null)
                    );
                }
                else
                {
                    if (!IsMatchesWithConfig(gallery, galleryData))
                    {
                        logger.Warning(
                            "Skipping gallery synchronization because configuration does not match existing gallery. " +
                            "GalleryId={GalleryId}, ExistingName={ExistingName}, ConfiguredName={ConfiguredName}, " +
                            "ExistingPath={ExistingPath}, ConfiguredPath={ConfiguredPath}", ApplicationArea.Tools,
                            gallery.Id,
                            gallery.Name,
                            galleryData.Name,
                            gallery.Path,
                            galleryData.Path
                        );

                        continue;
                    }

                    logger.Info(
                        "Synchronizing existing gallery {GalleryName} ({GalleryId})", ApplicationArea.Tools,
                        gallery.Name,
                        gallery.Id
                    );
                }

                ProgressUpdate(new SyncEvent(
                    SyncEventType.GallerySyncStarted,
                    gallery.Name,
                    null,
                    null)
                );

                await SyncGalleryAsync(gallery, files);

                await persistence.SaveChangesAsync();


                // gallery thumbnail
                string? thumbnailFile = galleryData.ThumbnailFile;
                if (requiresInitialThumbnail)
                {
                    thumbnailFile ??= files.First().Files.First();
                }

                if (thumbnailFile != null)
                {
                    var thumbnailAsset = await assetsService.GetByPathAsync(gallery.Id, thumbnailFile);
                    if (thumbnailAsset == null)
                    {
                        logger.Warning(
                            "Configured thumbnail asset was not found. GalleryId={GalleryId}, GalleryName={GalleryName}, AssetPath={AssetPath}", ApplicationArea.Tools,
                            gallery.Id,
                            gallery.Name,
                            thumbnailFile
                        );
                    }
                    else if (thumbnailAsset.Id != gallery.CoverAssetId)
                    {
                        await galleriesService.StageUpdatePreviewAssetAsync(gallery.Id, thumbnailAsset.Id);
                    }
                }

                logger.Info(
                    "Gallery synchronization completed for {GalleryName}. " +
                    "Files={TotalFilesToProcess}, " +
                    "CreatedAssets={CreatedAssets}, " +
                    "SkippedAssets={SkippedAssets}, " +
                    "DeletedAssets={DeletedAssets}, " +
                    "CreatedGroups={CreatedGroups}", ApplicationArea.Tools,
                    state[gallery.Name].GalleryName,
                    state[gallery.Name].TotalFilesToProcess,
                    state[gallery.Name].CreatedAssets,
                    state[gallery.Name].SkippedAssets,
                    state[gallery.Name].DeletedAssets,
                    state[gallery.Name].CreatedGroups);
            }

            logger.Info("Synchronization completed", ApplicationArea.Tools);
        }

        private async Task SyncGalleryAsync(GalleryDto gallery, IEnumerable<FilesGroup> filesGroups)
        {
            foreach (var filesGroup in filesGroups)
            {
                /// 0. root folder === no group
                ///    - add all assets
                /// 2. group do not exists
                ///   - create group
                ///   - create assets
                /// 1. group exists 
                ///   - verifiy files synchronization
                ///   - create/delete assets if needed
                ///   

                if (string.IsNullOrEmpty(filesGroup.Folder)) // no group --- root folder
                {
                    await CreateAssets(gallery, filesGroup.Files, null);
                    continue;
                }

                AssetGroupDto? group = await gropusService.GetPhysicalGroup(gallery.Id, filesGroup.Folder);
                if (group == null)
                {
                    await CreateGroupAsync(gallery, filesGroup);
                }
                else if (!gropusService.IsSynchronized(group, filesGroup.Files, out List<AssetSynchronizationDto> outOfSyncAsset))// out of sync
                {
                    await SyncGroupAsync(gallery, group, outOfSyncAsset);
                }
                else // fine and up to date 
                {
                    ProgressUpdate(new SyncEvent(
                        SyncEventType.GroupSyncSkipped,
                        gallery.Name,
                        group,
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

        private async Task CreateGroupAsync(GalleryDto gallery, FilesGroup filesGroup)
        {
            DateTime creationTime = Directory.GetCreationTimeUtc(
                Path.Combine(gallery.Path, filesGroup.Folder));

            AssetGroupDtoCreate createDto = new(gallery.Id, filesGroup.Folder, filesGroup.Folder, creationTime);
            AssetGroupDto group = await gropusService.CreateGroup(createDto);

            ProgressUpdate(new SyncEvent(
                SyncEventType.GroupCreated,
                gallery.Name,
                group,
                null)
            );

            await CreateAssets(gallery, filesGroup.Files, group);
        }

        private async Task SyncGroupAsync(GalleryDto gallery, AssetGroupDto group, List<AssetSynchronizationDto> outOfSyncAsset)
        {
            ProgressUpdate(new SyncEvent(
                        SyncEventType.GroupSyncStarted,
                        gallery.Name,
                        group,
                        null)
                    );

            List<int> assetIdsToDelete = new List<int>();
            List<string> missingAssetPaths = new List<string>();

            foreach (var asset in outOfSyncAsset)
            {
                if (asset.Type == App.Enum.SyncMismatchType.OnlyDb) // files were removed from file system
                {
                    assetIdsToDelete.Add(asset.id!.Value);
                }
                else if (asset.Type == App.Enum.SyncMismatchType.OnlyFileSystem) // new files added inside folder in file system
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
            int deleted = await assetsService.StageDeleteRangeAsync(assetIdsToDelete);

            // normalize group positions
            int groupPositionOffset;
            if (deleted > 0)
            {
                groupPositionOffset = 1 + await gropusService.StageNormalizePositionsAsync(group!.Id);
            }
            else
            {
                groupPositionOffset = await gropusService.AssetsCountAsync(group!.Id);
            }

            await CreateAssets(gallery, [.. missingAssetPaths], group, groupPositionOffset);
        }

        private async Task CreateAssets(GalleryDto gallery, string[] files, AssetGroupDto? group, int positionOffset = 0)
        {
            for (int i = 0; i < files.Length; i++)
            {
                string relativePath = files[i];
                if (await assetsService.Exists(gallery.Id, relativePath))
                {
                    logger.Debug(
                        "Skipping existing asset {AssetPath} in gallery {GalleryId}", ApplicationArea.Tools,
                        relativePath,
                        gallery.Id
                    );

                    ProgressUpdate(new SyncEvent(
                        SyncEventType.AssetSkipped,
                        gallery.Name,
                        group,
                        relativePath)
                    );

                    continue;
                }

                string assetFilePath = Path.Combine(gallery.Path, relativePath);
                string previewPath = await previewCreatorService.StageCreatePreviewAsync(gallery.Path, assetFilePath);
                ProgressUpdate(new SyncEvent(
                    SyncEventType.AssetCreated,
                    gallery.Name,
                    group,
                    relativePath)
                );

                AssetDtoCreate assetDtoCreate = new(gallery.Id, relativePath, previewPath, group?.Id, group == null ? null : i + positionOffset);
                await assetsService.StageCreateAssetAsync(assetDtoCreate);
            }
        }


        private static (IEnumerable<FilesGroup>, int) GetFiles(GaleryData data)
        {
            string root = @$"{data.Path}";
            if (!Directory.Exists(root))
            {
                return ([], 0);
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm", ".gif" };

            SearchOption searchOption = SearchOption.AllDirectories;

            IEnumerable<string> files = Directory
                .EnumerateFiles(root, "*.*", searchOption)
                .Where(f => extensions.Contains(
                    Path.GetExtension(f).ToLowerInvariant()));

            int filesCount = files.Count();

            IEnumerable<FilesGroup> groupedFiles = files
                .OrderBy(File.GetLastWriteTimeUtc)
                .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
                .GroupBy(f =>
                {
                    var relativePath = Path.GetRelativePath(root, f);
                    var folder = Path.GetDirectoryName(relativePath);

                    return folder ?? "";
                })
                .Select(x => new FilesGroup(
                    x.Key,
                    x.Select(f => Path.GetRelativePath(root, f))
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                        .ToArray())
                );


            return (groupedFiles, filesCount);
        }

        private static bool IsMatchesWithConfig(GalleryDto gallery, GaleryData galleryConfigData)
        {
            return string.Equals(gallery.Name, galleryConfigData.Name, StringComparison.OrdinalIgnoreCase) && gallery.Path == galleryConfigData.Path;
        }


        private record FilesGroup(string Folder, string[] Files);
    }
}
