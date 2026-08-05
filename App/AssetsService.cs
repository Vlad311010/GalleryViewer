using App.Dto;
using App.Extensions;
using Data.Entities;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {
        public async Task<AssetDto> AddAssetAsync(AssetDtoCreate dto)
        {
            Asset entity = new Asset
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType()
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

            /*foreach (var assetId in assets)
            {
                context.Attach(new Asset { Id = assetId, Group = entity });
            }*/

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
            /*context.AttachRange(
                assets.Select(assetId => new Asset { Id = assetId, Group = entity })
            );*/

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }
    }
}
