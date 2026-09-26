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
        Task AddTagsAsync(AssetAddTagsCommand command);
        Task StageCreateAssetAsync(AssetCreateCommand command);
        void StageDelete(AssetDeleteCommand command);
        Task<bool> ExistsAsync(AssetExistsQuery query);
        Task<AssetGroupInfoDto?> GetAssetGroupInfoAsync(AssetGroupInfoQuery query);
        Task<AssetTagsDto> GetAssetTagsAsync(AssetTagsQuery query);
        Task<AssetDto?> GetByPathAsync(int galleryId, string relativePath);
        Task RemoveTagAsync(AssetRemoveTagCommand command);
        bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto);
        IAsyncEnumerable<IReadOnlyList<AssetFileInfoDto>> GetAssetsInBatchesAsync(int galleryId, DateTime timeStamp, int batchSize);
    }
}
