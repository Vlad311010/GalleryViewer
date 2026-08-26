namespace App.Dto.Asset
{
    public record AssetDtoCreate(
        int GalleryId,
        string RelativePath,
        string? PreviewPath,
        int? groupId,
        int? groupPosition
    );
}
