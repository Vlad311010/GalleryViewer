using App;
using App.Dto.Filter;
using App.Dto.Tag;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{
    public class TagsController(TagsServices tagsServices) : BaseController
    {
        [HttpGet("search")]
        [ProducesResponseType<PagedData<TagSearchResponseModel>>(StatusCodes.Status200OK)]
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
    }
}
