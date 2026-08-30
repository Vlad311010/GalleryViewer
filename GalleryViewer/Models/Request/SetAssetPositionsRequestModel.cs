using Shared.Models;

namespace GalleryViewer.Models.Request
{
    public record SetAssetPositionsRequestModel(IEnumerable<AssetPosition> Positions);
}
