using App.Dto.Asset;
using App.Dto.Tag;
using System.Diagnostics.CodeAnalysis;

namespace App.Interfaces.Services
{
    public interface IAssetService
    {
        Task AddTags(int assetId, string[] tags);
        Task<AssetDto> StageCreateAssetAsync(AssetDtoCreate dto);
        Task<bool> DeleteAsync(int id);
        Task<int> StageDeleteRangeAsync(IEnumerable<int> ids);
        Task<bool> Exists(int galleryId, string relativePath);
        Task<AssetGroupInfoDto?> GetAssetGroupInfo(int assetId);
        Task<AssetTagsDto> GetAssetTags(int assetId);
        Task<AssetDto?> GetByPathAsync(int galleryId, string relativePath);
        Task RemoveTag(int assetId, string tag);
        bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto);
    }
}
