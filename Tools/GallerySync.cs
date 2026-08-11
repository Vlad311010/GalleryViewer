using App;
using App.Dto;
using App.Dto.Asset;
using App.Dto.Gallery;
using App.Dto.Group;
using App.PreviewCreation;
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

        public async Task SyncronizeGalleryAsync(GaleryInicializationData data)
        {
            IEnumerable<FilesGroup> files = GetFiles(data);
            // TODO: verify

            if (files.Count() == 0)
            {
                return; // TODO: Meaningfull error
            }


            GalleryDto? gallery = await galleriesService.GetByNameAsync(data.Name);
            if (gallery == null)
            {
                GalleryDtoCreate galleryCreate = new GalleryDtoCreate(data.Name, data.Path);
                gallery = await galleriesService.CreateAndSaveAsync(galleryCreate);
                Debug.Log($"Gallery created {gallery.Name} {gallery.Path}");
            }


            await AddAssetsAsync(gallery, files);

            await persistence.SaveChangesAsync();
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
                    Debug.Log($"New group {filesGroup.Folder}");
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
                            Debug.Log($"Asset to Remove Id:{asset.id!.Value}  group:{group!.Id}");
                            assetIdsToDelete.Add(asset.id!.Value);
                        }
                        else if (asset.Type == App.Enum.SyncMismatchType.OnlyFileSystem) // new files added inside folder in file system
                        {
                            Debug.Log($"Asset to Add group:{group!.Id}");
                            missingAssetPaths.Add(asset.RelativePath);
                        }
                    }

                    // delete
                    int deleted = await assetsService.DeleteRangeAsync(assetIdsToDelete);

                    // normalize group positions
                    int groupPositionOffset; // TODO: refactor/remove. calculate position on insert relay on actual group object.
                    if (deleted > 0)
                    {
                        groupPositionOffset = 1 + await gropusService.NormalizePositionAsync(group!.Id);
                    }
                    else
                    {
                        groupPositionOffset = await gropusService.AssetsCount(group!.Id);
                    }

                    await CreateAssets(gallery, new FilesGroup(filesGroup.Folder, [.. missingAssetPaths]), group, groupPositionOffset);
                    for (int i = 0; i < missingAssetPaths.Count; i++)
                    {
                        // create
                    }
                }
                else // fine and up to date 
                {
                    Debug.Log($"Group is insync id:{group.Id}");
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
                    Debug.Log($"Skip asset {relativePath}");
                    continue; // TODO:? maybe update some data
                }

                Debug.Log($"Create asset {relativePath}");
                string assetFilePath = Path.Combine(gallery.Path, relativePath);
                string previewPath = await previewCreatorService.CreatePreviewAsync(gallery.Path, assetFilePath);

                AssetDtoCreate assetDtoCreate = new(gallery.Id, relativePath, previewPath, group?.Id, group == null ? null : i + positionOffset);
                await assetsService.CreateAssetAsync(assetDtoCreate);
            }
        }

        private IEnumerable<FilesGroup> GetFiles(GaleryInicializationData data)
        {
            string root = @$"{data.Path}";

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm" };
            // var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

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


            foreach (var group in files)
            {
                Debug.Log($"[{group.Folder}]");

                foreach (var file in group.Files)
                {
                    Debug.Log($"\t{file}");
                }
            }

            return files;
        }

        private record FilesGroup(string Folder, string[] Files);
    }
}
