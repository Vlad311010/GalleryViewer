using Shared.Models;

namespace GalleryViewer.Models.Response
{
    public record AssetGroupResponseModel(
            int Id,
            int GalleryId,
            int CoverAssetPosition,
            bool isAssetAddRemoveAllowed,
            AssetPosition[] Positions,
            string? Title
        );
}
