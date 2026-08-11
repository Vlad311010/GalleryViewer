namespace App.Dto.Gallery
{
    public record GalleryDto(
        int Id,
        string Name,
        string Path
    ) : EntityDto(Id);
}
