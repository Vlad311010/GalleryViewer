using App.Commands;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using System.Diagnostics.CodeAnalysis;

namespace App.Interfaces.Services
{
    public interface IAssetService
    {
        Task AddTags(AssetAddTagsCommand command);
        Task<AssetDto> StageCreateAssetAsync(AssetCreateCommand command);
        Task<bool> DeleteAsync(int id);
        Task<int> StageDeleteRangeAsync(IEnumerable<int> ids);
        Task<bool> Exists(int galleryId, string relativePath);
        Task<AssetGroupInfoDto?> GetAssetGroupInfo(AssetGroupInfoQuery query);
        Task<AssetTagsDto> GetAssetTags(AssetTagsQuery query);
        Task<AssetDto?> GetByPathAsync(int galleryId, string relativePath);
        Task RemoveTag(AssetRemoveTagCommand command);
        bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto);
    }
}
