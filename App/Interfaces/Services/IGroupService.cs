using App.Models.Commands;
using App.Models.Dtos.Group;
using App.Models.Queries;

namespace App.Interfaces.Services
{
    public interface IGroupService
    {
        Task<int> AssetsCountAsync(AssetGroupQuery query);
        Task<AssetGroupDto> CreateGroup(CreateAssetGroupCommand command);
        Task<AssetGroupDtoWithAssetPositions> GetByIdAsync(AssetGroupQuery query);
        Task<AssetGroupDto?> GetPhysicalGroup(PhysicalAssetGroupQuery query);
        bool IsSynchronized(AssetGroupSynchronizationQuery query, out string[] unstagedFiles);
        Task StageNormalizePositionsAsync(AssetGroupQuery query);
        Task SetCover(SetGroupCoverCommand command);
        Task SetPositionsAsync(SetAssetsPositionsCommand command);
        Task DeleteAsync(GroupDeleteCommand query);
    }
}
