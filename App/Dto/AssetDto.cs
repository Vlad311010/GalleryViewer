namespace App.Dto
{
    public record AssetDto(
        int Id,
        int GalleryId,
        string RelativePath,
        DateTime CreationTime,
        DateTime ImportTime
    ) : EntityDto(Id);
}
