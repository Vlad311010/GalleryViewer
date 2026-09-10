using App.Commands;
using App.Dtos.Filter;
using App.Dtos.Tag;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface ITagsService
    {
        Task<TagDtoInfo> Create(CreateTagCommand tagDtoCreate);
        Task<TagDto> Get(int id);
        Task<PagedData<TagDtoInfo>> ListAsync(PaginationDto paginationDto);
        Task<IEnumerable<TagDtoSearch>> SearchAsync(string searchValue, int take);
    }
}
