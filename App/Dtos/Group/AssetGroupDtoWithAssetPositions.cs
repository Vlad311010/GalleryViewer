using Shared.Models;

namespace App.Dtos.Group
{
    public record AssetGroupDtoWithAssetPositions(
        int Id,
        int GalleryId,
        int CoverAssetIdx,
        string? Title,
        string? PhysicalPath,
        IEnumerable<AssetPosition> Positions
    );
}
