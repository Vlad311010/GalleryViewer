using App;
using App.Dto;
using Tools.Models;

namespace Tools
{
    internal class GalleryInitializer(GalleriesService galleriesService, AssetsService assetsService, PersistenceService persistence)
    {

        public async Task Inicizalize(InicializationData data)
        {
            IEnumerable<FilesGroup> files = GetFiles(data);
            // verify

            if (files.Count() == 0)
            {
                return; // Meaningfull error
            }

            GalleryDtoCreate galleryData = new GalleryDtoCreate(data.Name, data.Path);
            string galleryName = (await galleriesService.CreateAsync(galleryData)).Name; // Id not available until data is saved

            await persistence.SaveChangesAsync();
            // int galleryId = await CreateGalleryAsync(data);


            await AddAssetsAsync(galleryName, files);

            await persistence.SaveChangesAsync();
        }

        private async Task AddAssetsAsync(string galleryName, IEnumerable<FilesGroup> filesGroups)
        {
            GalleryDto gallery = await galleriesService.GetByNameAsync(galleryName);
            foreach (var group in filesGroups)
            {
                AssetDtoCreate[] assets = group.Files
                    .Select(path => new AssetDtoCreate(
                        gallery.Id,
                        path,
                        null,
                        null)
                    ).ToArray();

                if (string.IsNullOrEmpty(group.Folder)) // root folder
                {
                    foreach (var asset in assets)
                    {
                        await assetsService.AddAssetAsync(asset);
                    }
                }
                else
                {
                    await assetsService.CreateGroupAsync(group.Folder, assets);
                }
            }
        }



        private IEnumerable<FilesGroup> GetFiles(InicializationData data)
        {
            string root = @$"{data.Path}";

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm" };

            SearchOption searchOption = data.groupSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

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
                    Debug.Log($"  {file}");
                }
            }

            return files;
        }

        private record FilesGroup(string Folder, string[] Files);
    }
}
