using App.Dto.Asset;
using App.Dto.Gallery;
using App.Dto.Group;
using App.PreviewCreation;
using App.Services;
using Tools.Models;

namespace Tools
{
    internal class GallerySync(
        GalleriesService galleriesService,
        AssetsService assetsService,
        PreviewCreatorService previewCreatorService,
        GroupsService gropusService,
        PersistenceService persistence)
    {

        public async Task SyncronizeGalleryAsync(GallerySyncData syncData)
        {
            foreach (var galleryData in syncData.Galeries)
            {

                IEnumerable<FilesGroup> files = GetFiles(galleryData);

                if (files.Count() == 0)
                {
                    Loggining.Log($"Skipping {galleryData.Name} as {galleryData.Path} does not contain any file");
                    continue;
                }

                bool requiresInitialThumbnail = false;
                GalleryDto? gallery = await galleriesService.GetByNameAsync(galleryData.Name);
                if (gallery == null)
                {
                    requiresInitialThumbnail = true;
                    GalleryDtoCreate galleryCreate = new GalleryDtoCreate(galleryData.Name, galleryData.Path);
                    gallery = await galleriesService.CreateAndSaveAsync(galleryCreate);
                    Loggining.Log($"Created gallery {gallery.Name} with path: {gallery.Path}");
                }
                else
                {
                    if (!IsMatchesWithConfig(gallery, galleryData))
                    {
                        Loggining.Warning($"Skipping {gallery.Name}.\n" +
                            $"Existing gallery mistmaches with provided in config." +
                            $"\n{gallery.Name}=={galleryData.Name}\n{gallery.Path}=={galleryData.Path}");
                        continue;
                    }
                    Loggining.Log($"Gallery {gallery.Name} with path: {gallery.Path} alredy exists.\nSynchronize...");
                }


                await AddAssetsAsync(gallery, files);

                await persistence.SaveChangesAsync();

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
                        Loggining.Error($"Asset with path: {thumbnailFile} does not exist in gallery {gallery.Name}(Id: {gallery.Id})");
                    }
                    else if (thumbnailAsset.Id != gallery.CoverAssetId)
                    {
                        await galleriesService.SetPreviewAssetAsync(gallery.Id, thumbnailAsset.Id);
                        await persistence.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task AddAssetsAsync(GalleryDto gallery, IEnumerable<FilesGroup> filesGroups)
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
                    await CreateAssets(gallery, filesGroup, null);
                    continue;
                }

                AssetGroupDto? group = await gropusService.GetPhysicalGroup(gallery.Id, filesGroup.Folder);
                if (group == null)
                {
                    Loggining.Log($"New group {filesGroup.Folder}");
                    await CreateGroupAsync(gallery, filesGroup);
                }
                else if (!gropusService.IsSynchronized(group, filesGroup.Files, out List<AssetSynchronizationDto> outOfSyncAsset))// out of sync
                {
                    List<int> assetIdsToDelete = new List<int>();
                    List<string> missingAssetPaths = new List<string>();
                    foreach (var asset in outOfSyncAsset)
                    {
                        if (asset.Type == App.Enum.SyncMismatchType.OnlyDb) // files were removed from file system
                        {
                            Loggining.Log($"Asset to Remove Id:{asset.id!.Value}  group:{group!.Id}");
                            assetIdsToDelete.Add(asset.id!.Value);
                        }
                        else if (asset.Type == App.Enum.SyncMismatchType.OnlyFileSystem) // new files added inside folder in file system
                        {
                            Loggining.Log($"Asset to Add group:{group!.Id}");
                            missingAssetPaths.Add(asset.RelativePath);
                        }
                    }

                    // delete
                    int deleted = await assetsService.DeleteRangeAsync(assetIdsToDelete);

                    // normalize group positions
                    int groupPositionOffset;
                    if (deleted > 0)
                    {
                        groupPositionOffset = 1 + await gropusService.NormalizePositionsAsync(group!.Id);
                    }
                    else
                    {
                        groupPositionOffset = await gropusService.AssetsCountAsync(group!.Id);
                    }

                    await CreateAssets(gallery, new FilesGroup(filesGroup.Folder, [.. missingAssetPaths]), group, groupPositionOffset);

                }
                else // fine and up to date 
                {
                    Loggining.Log($"Group is insync id:{group.Id}");
                }
            }
        }

        private async Task CreateGroupAsync(GalleryDto gallery, FilesGroup filesGroup)
        {
            DateTime creationTime = Directory.GetCreationTimeUtc(
                Path.Combine(gallery.Path, filesGroup.Folder));

            AssetGroupDtoCreate createDto = new(gallery.Id, filesGroup.Folder, filesGroup.Folder, creationTime);
            AssetGroupDto group = await gropusService.CreateGroupAndSaveAsync(createDto);

            await CreateAssets(gallery, filesGroup, group);
        }

        private async Task CreateAssets(GalleryDto gallery, FilesGroup filesGroup, AssetGroupDto? group, int positionOffset = 0)
        {
            for (int i = 0; i < filesGroup.Files.Length; i++)
            {
                string relativePath = filesGroup.Files[i];
                if (await assetsService.Exists(gallery.Id, relativePath))
                {
                    Loggining.Log($"\tSkip asset {relativePath}");
                    continue; // TODO:? maybe update some data
                }

                Loggining.Log($"\t Create asset {relativePath}");
                string assetFilePath = Path.Combine(gallery.Path, relativePath);
                string previewPath = await previewCreatorService.CreatePreviewAsync(gallery.Path, assetFilePath);

                AssetDtoCreate assetDtoCreate = new(gallery.Id, relativePath, previewPath, group?.Id, group == null ? null : i + positionOffset);
                await assetsService.CreateAssetAsync(assetDtoCreate);
            }
        }

        private static IEnumerable<FilesGroup> GetFiles(GaleryData data)
        {
            string root = @$"{data.Path}";
            if (!Directory.Exists(root))
            {
                return [];
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm", ".gif" };

            SearchOption searchOption = SearchOption.AllDirectories;

            IEnumerable<FilesGroup> files = Directory
                .EnumerateFiles(root, "*.*", searchOption)
                .Where(f => extensions.Contains(
                    Path.GetExtension(f).ToLowerInvariant()))
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


            /*foreach (var group in files)
            {
                Loggining.Log($"[{group.Folder}]");

                foreach (var file in group.Files)
                {
                    Loggining.Log($"\t{file}");
                }
            }*/

            return files;
        }

        private static bool IsMatchesWithConfig(GalleryDto gallery, GaleryData galleryConfigData)
        {
            return string.Equals(gallery.Name, galleryConfigData.Name, StringComparison.OrdinalIgnoreCase) && gallery.Path == galleryConfigData.Path;
        }

        private record FilesGroup(string Folder, string[] Files);
    }
}
