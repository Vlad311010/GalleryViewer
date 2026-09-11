using App.Models;
using App.Models.Commands;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using Shared.Models;

namespace App.Interfaces.Services
{
    public interface ITagsService
    {
        Task<TagDtoInfo> Create(CreateTagCommand createTagCommand);
        Task<TagDto> Get(int id);
        Task<PagedData<TagDtoInfo>> ListAsync(Pagination pagination);
        Task<IEnumerable<TagDtoSearch>> SearchAsync(TagSearchQuery query);
    }
}
