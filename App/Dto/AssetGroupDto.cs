namespace App.Dto
{
    public record AssetGroupDto(
        int Id,
        int GalleryId,
        int CoverAssetIdx,
        string? Title,
        string? PhysicalPath
    );
}
