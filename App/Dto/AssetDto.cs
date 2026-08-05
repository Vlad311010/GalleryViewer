namespace App.Dto
{
    public record AssetDto(
        int Id,
        int GalleryId,
        string RelativePath
    ) : EntityDto(Id);
}
