namespace App.Dto
{
    public record GalleryDto(
        int Id,
        string Name,
        string Path,
        DateTime CreationTime,
        DateTime ImportTime
    ) : EntityDto(Id);
}
