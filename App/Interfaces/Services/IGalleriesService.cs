using App.Commands;
using App.Dtos.Gallery;

namespace App.Interfaces.Services
{
    public interface IGalleriesService
    {
        Task<GalleryDto> Create(GalleryCreateCommand command);
        Task<GalleryDto?> GetByNameAsync(string galleryName);
        Task<IEnumerable<GalleryDto>> ListAsync();
        Task StageUpdatePreviewAssetAsync(int galleryId, int coverSourceId);
    }
}
