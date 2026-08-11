namespace App.Dto.Asset
{
    public record AssetDto(
        int Id,
        int GalleryId,
        string RelativePath,
        string? PreviewPath,
        DateTime CreationTime,
        DateTime ImportTime
    ) : EntityDto(Id);
}
