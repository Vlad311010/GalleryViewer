using App.Models.Dtos;

namespace App.Models.Dtos.Gallery
{
    public record GalleryDto(
        int Id,
        string Name,
        string Path,
        int? CoverAssetId
    ) : EntityDto(Id);
}
