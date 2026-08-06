namespace App.Dto
{
    public record GalleryDto(
        int Id,
        string Name,
        string Path
    ) : EntityDto(Id);
}
