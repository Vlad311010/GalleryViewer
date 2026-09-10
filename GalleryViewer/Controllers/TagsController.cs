using App.Commands;
using App.Dtos.Filter;
using App.Dtos.Tag;
using App.Interfaces.Services;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class TagsController(ITagsService tagsServices) : BaseController
    {
        [HttpGet("search")]
        [EndpointName("searchTags")]
        [ProducesResponseType<IEnumerable<TagSearchResponseModel>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string value, [FromQuery] int take = 5)
        {
            var tags = await tagsServices.SearchAsync(value, take);

            return Ok(
                tags.Select(x => new TagSearchResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category,
                    IsCanonical = x.IsCanonical,
                    CanonicalName = x.CanonicalName,
                    Occurrences = x.Occurrences
                }).ToArray()
            );
        }

        [HttpGet("list")]
        [EndpointName("listTags")]
        [ProducesResponseType<PagedData<TagInfoResponseModel>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] BaseFilterRequestModel request)
        {
            PaginationDto filter = new() { Skip = request.Skip, Take = request.Take };
            PagedData<TagDtoInfo> result = await tagsServices.ListAsync(filter);

            return Ok(
                result.Cast(x => new TagInfoResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category,
                    Occurrences = x.Occurrences,
                    CanonicalId = x.CanonicalId,
                })
            );
        }

        [HttpPost]
        [EndpointName("tagCreate")]
        [ProducesResponseType<TagCreateResponseModel>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] TagCreateRequestModel request)
        {
            CreateTagCommand command = new(request.Name, request.Category, null);
            TagDtoInfo tag = await tagsServices.Create(command);

            return Created((string)null!, new TagCreateResponseModel(tag.Id, tag.Name, tag.Category));
        }
    }
}
