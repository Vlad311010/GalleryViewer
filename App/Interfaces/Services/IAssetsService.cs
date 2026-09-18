using App.Commands;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using System.Diagnostics.CodeAnalysis;

namespace App.Interfaces.Services
{
    public interface IAssetsService
    {
        Task AddTags(AssetAddTagsCommand command);
        Task<AssetDto> StageCreateAssetAsync(AssetCreateCommand command);
        void StageDelete(AssetDeleteCommand command);
        Task<bool> ExistsAsync(int galleryId, string relativePath);
        Task<AssetGroupInfoDto?> GetAssetGroupInfo(AssetGroupInfoQuery query);
        Task<AssetTagsDto> GetAssetTags(AssetTagsQuery query);
        Task<AssetDto?> GetByPathAsync(int galleryId, string relativePath);
        Task RemoveTag(AssetRemoveTagCommand command);
        bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto);
        IAsyncEnumerable<IReadOnlyList<AssetFileInfoDto>> GetAssetsInBatchesAsync(int galleryId, DateTime timeStamp, int batchSize);
    }
}
