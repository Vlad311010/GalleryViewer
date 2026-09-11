using App.Models.Dtos.Asset;
using Data.Entities;

namespace App.Mappers
{
    public static class AssetMapper
    {
        public static AssetDto ToAssetDto(this Asset asset)
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

        public static AssetGroupInfoDto ToAssetGroupInfoDto(this Asset asset, int[]? groupAssets)
        {
            ArgumentNullException.ThrowIfNull(asset);

            return new AssetGroupInfoDto(
                asset.Id,
                asset.GroupId,
                asset.GroupPosition,
                groupAssets
            );
        }


    }
}
