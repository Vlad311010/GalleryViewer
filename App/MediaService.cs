using App.Dto;
using App.Enum;
using App.Exceptions;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App
{
    public class MediaService(AssetsCatalogContext context)
    {
        public async Task<MediaDto> GetAssetPreviewAsync(MediaDtoFetch mediaRequest)
        {
            FileInfo previeFileInfo = null;
            switch (mediaRequest.ItemType)
            {
                case DisplayItemType.Asset:
                    previeFileInfo = await GetAssetPreviewPathAsync(mediaRequest.ItemId);
                    break;
                case DisplayItemType.Group:
                    previeFileInfo = await GetGroupPreviewPathAsync(mediaRequest.ItemId);
                    break;
            }

            if (previeFileInfo == null || string.IsNullOrWhiteSpace(previeFileInfo.Path) || !File.Exists(previeFileInfo.Path))
            {
                // TODO: return not found preview image
            }

            return new MediaDto(
                new FileStream(previeFileInfo.Path, FileMode.Open, FileAccess.Read, FileShare.Read),
                previeFileInfo.MimeType
            );
        }

        public async Task<MediaDto> GetAssetMediaAsync(AssetMediaDtoFetch assetRequest)
        {
            Asset? asset = await context.Assets.FindAsync(assetRequest.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetRequest.AssetId);

            Gallery? gallery = await context.Galleries.FindAsync(asset.GalleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(gallery, asset.GalleryId);

            string assetPath = Path.Combine(gallery.Path, asset.RelativePath);
            if (string.IsNullOrWhiteSpace(assetPath) || !File.Exists(assetPath))
            {
                // TODO: return not found preview image
            }

            return new MediaDto(
                new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                asset.MimeType
            );
        }


        private async Task<FileInfo> GetAssetPreviewPathAsync(int id)
        {
            Asset? asset = await context.Assets.FindAsync(id);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, id);

            return new FileInfo(asset.PreviewPath, asset.MimeType);
        }


        private async Task<FileInfo> GetGroupPreviewPathAsync(int id)
        {
            var fileInfo = await context.AssetGroups
                .Where(g => g.Id == id)
                .Select(g =>
                    g.Assets
                        .Where(a => a.GroupPosition == g.CoverAssetIdx)
                        .Select(a => new FileInfo
                        (
                            a.PreviewPath,
                            a.MimeType
                        ))
                        .Single()
                )
                .SingleOrDefaultAsync();

            if (fileInfo == null)
            {
                throw new EntityNotFoundException<AssetGroup>(id);
            }

            return fileInfo;
        }

        private record FileInfo(string? Path, string MimeType);
    }
}
