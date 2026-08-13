using App.Dto.Asset;
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
    }
}
