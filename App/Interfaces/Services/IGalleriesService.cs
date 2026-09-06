using App.Dto.Gallery;

namespace App.Interfaces.Services
{
    public interface IGalleriesService
    {
        Task<GalleryDto> CreateAndSaveAsync(GalleryDtoCreate dto);
        Task<GalleryDto?> GetByNameAsync(string galleryName);
        Task<IEnumerable<GalleryDto>> ListAsync();
        Task SetPreviewAssetAsync(int galleryId, int coverSourceId);
    }
}
