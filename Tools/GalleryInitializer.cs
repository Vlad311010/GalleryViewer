using App;
using App.Dto;
using App.PreviewCreation;
using Tools.Models;

namespace Tools
{
    internal class GalleryInitializer(
        GalleriesService galleriesService,
        AssetsService assetsService,
        PreviewCreatorService previewCreatorService,
        PersistenceService persistence)
    {

        public async Task InicializeGallery(GaleryInicializationData data)
        {
            IEnumerable<FilesGroup> files = GetFiles(data);
            // TODO: verify

            if (files.Count() == 0)
            {
                return; // TODO: Meaningfull error
            }

            GalleryDtoCreate galleryData = new GalleryDtoCreate(data.Name, data.Path);
            GalleryDto gallery = await galleriesService.CreateAndSaveAsync(galleryData);

            await AddAssetsAsync(gallery, files);

            await persistence.SaveChangesAsync();
        }

        private async Task AddAssetsAsync(GalleryDto gallery, IEnumerable<FilesGroup> filesGroups)
        {
            foreach (var filesGroup in filesGroups)
            {
                AssetGroupDto? group = null;
                if (!string.IsNullOrEmpty(filesGroup.Folder)) // skip grouping for root folder
                {
                    DateTime creationTime = Directory.GetCreationTimeUtc(
                        Path.Combine(gallery.Path, filesGroup.Folder));

                    group = await assetsService.CreateGroupAndSaveAsync(
                        filesGroup.Folder,
                        creationTime);
                }


                for (int i = 0; i < filesGroup.Files.Length; i++)
                {
                    string assetFilePath = Path.Combine(gallery.Path, filesGroup.Files[i]);
                    string previewPath = await previewCreatorService.CreatePreviewAsync(gallery.Path, assetFilePath);

                    AssetDtoCreate assetDtoCreate = new AssetDtoCreate(gallery.Id, filesGroup.Files[i], previewPath);
                    if (group == null)
                    {
                        await assetsService.CreateAssetAsync(assetDtoCreate);
                    }
                    else
                    {
                        await assetsService.CreateAssetAsync(assetDtoCreate, group.Id, i);
                    }
                }
            }
        }



        private IEnumerable<FilesGroup> GetFiles(GaleryInicializationData data)
        {
            string root = @$"{data.Path}";

            // var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm" };
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            SearchOption searchOption = SearchOption.AllDirectories;

            IEnumerable<FilesGroup> files = Directory
                .EnumerateFiles(root, "*.*", searchOption)
                .Where(f => extensions.Contains(
                    Path.GetExtension(f).ToLowerInvariant()))
                .GroupBy(f =>
                {
                    var relativePath = Path.GetRelativePath(root, f);
                    var folder = Path.GetDirectoryName(relativePath);

                    return folder ?? "";
                })
                .Select(x => new FilesGroup(x.Key, x.Select(f => Path.GetRelativePath(root, f)).ToArray()));


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
