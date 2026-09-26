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

        public async Task<string> GetAssetMimeTypeAsync(AssetQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetQueryValidator().ValidateAndThrowAsync(query);

            Asset? asset = await context.Assets.FindAsync(query.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, query.AssetId);

            return asset.MimeType;
        }

        public async Task<MediaDto> GetPreviewAsync(PreviewQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new PreviewQueryValidator().ValidateAndThrowAsync(query);

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

            if (!mediaAccessorService.Exists(previeFilePath))
            {
                throw new MediaNotFoundException($"Preview file for {query.ItemType} {query.ItemId} not found.", previeFilePath);
            }

            return new MediaDto(
                mediaAccessorService.GetMediaData(previeFilePath),
                previeFilePath.ToMimeType()
            );
        }


        private async Task<string?> GetAssetPreviewPathAsync(int id)
        {
            Asset? asset = await context.Assets.FindAsync(id);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, id);

            return asset?.PreviewPath;
        }

        private async Task<string?> GetGroupPreviewPathAsync(int id)
        {
            AssetGroup? group = await context.AssetGroups.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, id);

            Asset? asset = await context.Assets
                .Where(a =>
                    a.GroupId == id &&
                    a.GroupPosition == group.CoverAssetIdx)
                .SingleOrDefaultAsync();

            EntityNotFoundException<Asset>.ThrowIfNull(asset, string.Empty);

            return asset.PreviewPath;
        }
    }
}
