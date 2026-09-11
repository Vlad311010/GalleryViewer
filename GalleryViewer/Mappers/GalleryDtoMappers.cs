using App.Models.Dtos.Gallery;
using GalleryViewer.Models.Response;

namespace GalleryViewer.Mappers
{
    public static class GalleryDtoMappers
    {
        public static GalleryResponseModel ToGalleryResponseModel(this GalleryDto dto)
        {
            return new(dto.Id, dto.Name, dto.CoverAssetId);
        }
    }
}
