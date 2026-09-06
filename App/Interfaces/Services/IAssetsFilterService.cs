using App.Dto.Filter;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface IAssetsFilterService
    {
        Task<PagedData<DisplayItemDto>> ListAsync(string galleryName, PaginationDto filter, TagFiltersDto tagFilters);
        Task<PagedData<DisplayItemDto>> ListGroupAssetsAsync(int groupId, PaginationDto filter);
    }
}
