namespace GalleryViewer.Models.Response
{
    public record AssetGroupPositionResponseModel(
        int AssetId,
        bool IsInGroup,
        int? GroupId,
        int? PreviousAsset,
        int? NextAsset
    );
}
