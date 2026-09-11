using App.Interfaces.Services;
using App.Models;
using App.Models.Queries;
using GalleryViewer.Helpers;
using GalleryViewer.Mappers;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class AssetBrowserController(IAssetsFilterService filterService) : BaseController
    {
        [HttpGet("{gallery}")]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listItems")]
        public async Task<IActionResult> ListGalleryItems([FromRoute] string gallery, [FromQuery] GalleryFilterRequestModel request)
        {
            Pagination pagination = new(request.Skip, request.Take);
            TagFilters tagFilters = new TagFilters
            {
                Tags = request.Tags?.Select(x => x.NormalizeTag()).ToArray() ?? [],
                ExcludeTags = request.ExcludeTags?.Select(x => x.NormalizeTag()).ToArray() ?? []
            };

            ListAssetsQuery query = new(gallery, pagination, tagFilters);
            var result = await filterService.ListAsync(query);

            return Ok(
                result.Cast(x => x.ToDisplayItemResponseModel())
            );
        }


        [HttpGet("group/{groupId}")]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listGroup")]
        public async Task<IActionResult> ListGroupItems(
                [FromRoute] int groupId,
                [FromQuery] PaginationRequestModel filterRequest
            )
        {
            Pagination pagination = new(filterRequest.Skip, filterRequest.Take);
            ListGroupAssetsQuery query = new(groupId, pagination);
            var result = await filterService.ListGroupAssetsAsync(query);

            return Ok(
                result.Cast(x => x.ToDisplayItemResponseModel())
            );
        }
    }
}
