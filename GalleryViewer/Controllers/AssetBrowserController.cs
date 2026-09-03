using App.Dto.Filter;
using App.Services;
using GalleryViewer.Helpers;
using GalleryViewer.Mappers;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class AssetBrowserController(FilterService filterService) : BaseController
    {
        [HttpGet("{gallery}")]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listItems")]
        public async Task<IActionResult> ListGalleryItems([FromRoute] string gallery, [FromQuery] GalleryFilterRequestModel request)
        {
            PaginationDto filter = new() { Skip = request.Skip, Take = request.Take };
            TagFiltersDto tagFilters = new TagFiltersDto
            {
                Tags = request.Tags?.Select(x => x.NormalizeTag()).ToArray() ?? [],
                ExcludeTags = request.ExcludeTags?.Select(x => x.NormalizeTag()).ToArray() ?? []
            };

            var result = await filterService.ListAsync(gallery, filter, tagFilters);

            return Ok(
                result.Cast(x => x.ToDisplayItemResponseModel())
            );
        }


        [HttpGet("group/{groupId}")]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listGroup")]
        public async Task<IActionResult> ListGroupItems(
                [FromRoute] int groupId,
                [FromQuery] BaseFilterRequestModel filterRequest
            )
        {
            PaginationDto filter = new() { Skip = filterRequest.Skip, Take = filterRequest.Take };
            var result = await filterService.ListGroupAssetsAsync(groupId, filter);

            return Ok(
                result.Cast(x => x.ToDisplayItemResponseModel())
            );
        }
    }
}
