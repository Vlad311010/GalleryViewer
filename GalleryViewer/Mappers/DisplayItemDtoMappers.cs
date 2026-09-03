using App.Dto.Filter;
using GalleryViewer.Models.Response;

namespace GalleryViewer.Mappers
{
    public static class DisplayItemDtoMappers
    {
        public static DisplayItemResponseModel ToDisplayItemResponseModel(this DisplayItemDto dto)
        {
            return new DisplayItemResponseModel
            {
                Type = dto.Type,
                Id = dto.Id,
                CreationTime = dto.CreationTime,
                ImportTime = dto.ImportTime,
                Title = dto.Title,
                Count = dto.Count,
            };
        }
    }
}
