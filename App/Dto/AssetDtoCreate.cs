namespace App.Dto
{
    public record AssetDtoCreate(
        int GalleryId,
        string RelativePath,
        int? GroupId,
        int? GroupPosition
    );
}
