using App.Dto.Media;
using App.Enum;
using App.Exceptions;
using App.Extensions;
using App.Interfaces.Services;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Services
{
    public class MediaService(AssetsCatalogContext context, IMediaAccessorService mediaAccessorService) : IMediaService
    {
        public async Task<MediaDto> GetAssetMediaAsync(AssetMediaDtoFetch assetRequest)
        {
            Asset? asset = await context.Assets.FindAsync(assetRequest.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetRequest.AssetId);

            Gallery? gallery = await context.Galleries.FindAsync(asset.GalleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(gallery, asset.GalleryId);

            string assetPath = Path.Combine(gallery.Path, asset.RelativePath);
            if (!mediaAccessorService.Exists(assetPath))
            {
                throw new MediaNotFoundException("Media file not found", assetPath);
            }

            return new MediaDto(
                mediaAccessorService.GetMediaData(assetPath),
                asset.MimeType
            );
        }

        public async Task<string> GetAssetMimeType(AssetMediaDtoFetch assetRequest)
        {
            Asset? asset = await context.Assets.FindAsync(assetRequest.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetRequest.AssetId);

            return asset.MimeType;
        }

        public async Task<MediaDto> GetAssetPreviewAsync(MediaDtoFetch mediaRequest)
        {
            string? previeFilePath = null;
            switch (mediaRequest.ItemType)
            {
                case DisplayItemType.Asset:
                    previeFilePath = await GetAssetPreviewPathAsync(mediaRequest.ItemId);
                    break;
                case DisplayItemType.Group:
                    previeFilePath = await GetGroupPreviewPathAsync(mediaRequest.ItemId);
                    break;
            }

            if (previeFilePath == null || !mediaAccessorService.Exists(previeFilePath))
            {
                throw new MediaNotFoundException($"Preview file for {mediaRequest.ItemType} {mediaRequest.ItemId} not found.", previeFilePath);
            }

            return new MediaDto(
                new FileStream(previeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                previeFilePath.ToMimeType()
            );
        }


        private async Task<string?> GetAssetPreviewPathAsync(int id)
        {
            Asset? asset = await context.Assets.FindAsync(id);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, id);

            return asset.PreviewPath;
        }

        private async Task<string?> GetGroupPreviewPathAsync(int id)
        {
            string? previewPath = await context.AssetGroups
                .Where(g => g.Id == id)
                .Select(g =>
                    g.Assets
                        .Where(a => a.GroupPosition == g.CoverAssetIdx)
                        .Select(a =>
                            a.PreviewPath
                        )
                        .Single()
                )
                .SingleOrDefaultAsync();

            return previewPath;
        }
    }
}
