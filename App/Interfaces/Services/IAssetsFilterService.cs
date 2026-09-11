using App.Models.Dtos.Filter;
using App.Models.Queries;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface IAssetsFilterService
    {
        Task<PagedData<DisplayItemDto>> ListAsync(ListAssetsQuery query);
        Task<PagedData<DisplayItemDto>> ListGroupAssetsAsync(ListGroupAssetsQuery query);
    }
}
