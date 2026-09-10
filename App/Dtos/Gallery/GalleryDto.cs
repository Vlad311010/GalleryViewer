namespace App.Dtos.Gallery
{
    public record GalleryDto(
        int Id,
        string Name,
        string Path,
        int? CoverAssetId
    ) : EntityDto(Id);
}
