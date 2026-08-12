namespace GalleryViewer.Models.Response
{
    public record GalleryResponseModel(
        int Id,
        string Name,
        int? CoverAssetId
    );
}
