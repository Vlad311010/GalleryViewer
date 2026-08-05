using App.Dto;
using App.Extensions;
using Data.Entities;
using System.Security.Cryptography;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {
        public async Task<AssetDto> AddAssetAsync(AssetDtoCreate dto)
        {
            Gallery? targetGallery = context.Galleries.SingleOrDefault(x => x.Id == dto.GalleryId);
            if (targetGallery == null)
            {
                throw new ArgumentException("TODO: custom exception");
            }

            string hash = await ComputeMd5HashAsync(Path.Combine(targetGallery.Path, dto.RelativePath));
            Asset entity = new Asset
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType(),
                Hash = hash
            };

            entity = (await context.Assets.AddAsync(entity)).Entity;
            return new AssetDto(entity.Id, entity.GalleryId, entity.RelativePath);
        }


        public async Task<AssetGroupDto> CreateGroupAsync(string? groupName)
        {
            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                Title = string.IsNullOrWhiteSpace(groupName) ? null : groupName
            };

            entity = (await context.AssetGroups.AddAsync(entity)).Entity;

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }

        public async Task<AssetGroupDto> CreateGroupAsync(string? groupName, IList<AssetDtoCreate> assets)
        {
            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                Title = string.IsNullOrWhiteSpace(groupName) ? null : groupName
            };

            context.AssetGroups.Add(entity);
            entity = (await context.AssetGroups.AddAsync(entity)).Entity;


            for (int i = 0; i < assets.Count(); i++)
            {
                Asset assetEntity = new Asset
                {
                    GalleryId = assets[i].GalleryId,
                    RelativePath = assets[i].RelativePath,
                    MimeType = assets[i].RelativePath.ToMimeType(),
                    Group = entity,
                    GroupPosition = i
                };

                await context.Assets.AddAsync(assetEntity);
            }

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }

        private async Task<string> ComputeMd5HashAsync(string filePath) // TODO: extract to helper
        {
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(filePath);

            byte[] hash = await md5.ComputeHashAsync(stream);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
