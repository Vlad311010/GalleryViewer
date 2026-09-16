using App.Enums;
using App.Exceptions;
using App.Extensions;
using App.Interfaces.Services;
using App.Models.Dtos.Media;
using App.Models.Queries;
using App.Validators;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Services
{
    public class MediaService(AssetsCatalogContext context, IMediaAccessorService mediaAccessorService) : IMediaService
    {
        public async Task<MediaDto> GetAssetMediaAsync(AssetQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetQueryValidator().ValidateAndThrowAsync(query);

            Asset? asset = await context.Assets.FindAsync(query.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, query.AssetId);

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

        public async Task<string> GetAssetMimeType(AssetQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetQueryValidator().ValidateAndThrowAsync(query);

            Asset? asset = await context.Assets.FindAsync(query.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, query.AssetId);

            return asset.MimeType;
        }

        public async Task<MediaDto> GetPreviewAsync(MediaQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new MediaQueryValidator().ValidateAndThrowAsync(query);

            string? previeFilePath = null;
            switch (query.ItemType)
            {
                case DisplayItemType.Asset:
                    previeFilePath = await GetAssetPreviewPathAsync(query.ItemId);
                    break;
                case DisplayItemType.Group:
                    previeFilePath = await GetGroupPreviewPathAsync(query.ItemId);
                    break;
            }

            if (previeFilePath == null || !mediaAccessorService.Exists(previeFilePath))
            {
                throw new MediaNotFoundException($"Preview file for {query.ItemType} {query.ItemId} not found.", previeFilePath);
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
