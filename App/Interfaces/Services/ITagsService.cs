using App.Dto.Filter;
using App.Dto.Tag;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface ITagsService
    {
        Task<TagDtoInfo> Create(TagDtoCreate tagDtoCreate);
        Task<TagDto> Get(int id);
        Task<PagedData<TagDtoInfo>> ListAsync(PaginationDto paginationDto);
        Task<IEnumerable<TagDtoSearch>> SearchAsync(string searchValue, int take);
    }
}
