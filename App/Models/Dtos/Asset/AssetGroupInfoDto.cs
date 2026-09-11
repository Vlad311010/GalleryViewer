namespace App.Models.Dtos.Asset
{
    public record AssetGroupInfoDto(
        int AssetId,
        int? GroupId,
        int? GroupPosition,
        IReadOnlyList<int> groupAssets
    );
}
