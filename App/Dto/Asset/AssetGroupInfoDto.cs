namespace App.Dto.Asset
{
    public record AssetGroupInfoDto(
        int AssetId,
        int? GroupId,
        int? GroupPosition,
        IReadOnlyList<int> groupAssets
    );
}
