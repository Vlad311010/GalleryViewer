using App.Dto.Gallery;

namespace App.Interfaces.Services
{
    public interface IGalleriesService
    {
        Task<GalleryDto> Create(GalleryDtoCreate dto);
        Task<GalleryDto?> GetByNameAsync(string galleryName);
        Task<IEnumerable<GalleryDto>> ListAsync();
        Task StageUpdatePreviewAssetAsync(int galleryId, int coverSourceId);
    }
}
