using App.Dto.Asset;
using App.Dto.Group;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface IGroupService
    {
        Task<int> AssetsCountAsync(int groupId);
        Task<AssetGroupDto> CreateGroup(AssetGroupDtoCreate dto);
        Task<AssetGroupDtoWithAssetPositions> GetByIdAsync(int groupId);
        Task<AssetGroupDto?> GetPhysicalGroup(int galleryId, string physicalPath);
        bool IsSynchronized(AssetGroupDto group, string[] files, out List<AssetSynchronizationDto> outOfSyncFiles);
        Task<int> StageNormalizePositionsAsync(int groupId);
        Task SetCover(int groupId, int assetId);
        Task SetPositionsAsync(int groupId, IEnumerable<AssetPosition> positions);
    }
}
