using App.Dto;
using App.Dto.Asset;
using App.Extensions;
using App.Utils;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {
        private static AssetDto ToAssetDto(Asset asset) // move to extensions
        {
            ArgumentNullException.ThrowIfNull(asset);

            return new AssetDto(
                asset.Id,
                asset.GalleryId,
                asset.RelativePath,
                asset.PreviewPath,
                asset.CreationTime,
                asset.ImportTime
            );
        }

        public bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto)
        {
            Asset? asset = context.Assets.FirstOrDefault(x => x.Hash == md5Hash);
            if (asset == null)
            {
                assetDto = null!;
                return false;
            }

            assetDto = ToAssetDto(asset);
            return true;
        }

        public async Task<AssetDto> CreateAssetAsync(AssetDtoCreate dto)
        {
            Gallery? targetGallery = await context.Galleries.FindAsync(dto.GalleryId);
            if (targetGallery == null)
            {
                throw new ArgumentException("TODO: custom exception");
            }

            string assetFilePath = Path.Combine(targetGallery.Path, dto.RelativePath);
            if (!File.Exists(assetFilePath))
            {
                throw new FileNotFoundException("Asset file not found", dto.RelativePath);
            }

            /// As file creation time is reseted during copy, so modified time may be better source of true of when file landed in file system.
            /// Just for safety measure smallest of two values is taken as creationTime.
            DateTime creationTime = File.GetCreationTimeUtc(assetFilePath);
            DateTime modifiedTime = File.GetLastWriteTimeUtc(assetFilePath);
            creationTime = creationTime > modifiedTime ? modifiedTime : creationTime;
            DateTime currentTime = DateTime.UtcNow;
            string hash = await Md5Hash.ComputeAsync(assetFilePath);

            Asset entity = new()
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType(),
                Hash = hash,
                CreationTime = creationTime,
                ImportTime = currentTime,
                PreviewPath = dto.PreviewPath,

                GroupId = dto.groupId,
                GroupPosition = dto.groupPosition
            };

            entity = (await context.Assets.AddAsync(entity)).Entity;
            return ToAssetDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            int deleted = await context.Assets
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<int> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count() == 0)
            {
                return 0;
            }

            int deleted = await context.Assets
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync();

            return deleted;
        }

        public async Task<bool> Exists(int galleryId, string relativePath)
        {
            return await context.Assets
                .AnyAsync(x =>
                x.GalleryId == galleryId
                && x.RelativePath == relativePath);
        }
    }
}
