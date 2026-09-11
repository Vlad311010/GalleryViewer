using App.Models.Commands;
using App.Models.Dtos.Gallery;
using App.Models.Queries;

namespace App.Interfaces.Services
{
    public interface IGalleriesService
    {
        Task<GalleryDto> Create(GalleryCreateCommand command);
        Task<GalleryDto?> GetByNameAsync(GalleryByNameQuery query);
        Task<IEnumerable<GalleryDto>> ListAsync();
        Task StageUpdatePreviewAssetAsync(int galleryId, int coverSourceId);
    }
}
