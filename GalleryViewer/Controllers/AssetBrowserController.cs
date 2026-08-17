using App;
using App.Dto.Filter;
using GalleryViewer.Helpers;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{

    public class AssetBrowserController(FilterService filterService, AssetsService assetsService) : BaseController
    {
        [HttpGet]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listItems")]
        public async Task<IActionResult> ListGalleryItems([FromQuery] GalleryFilterRequestModel request)
        {
            PaginationDto filter = new() { Skip = request.Skip, Take = request.Take };
            TagFiltersDto tagFilters = new TagFiltersDto
            {
                Tags = request.Tags?.Select(x => x.NormalizeTag()).ToArray() ?? [],
                ExcludeTags = request.ExcludeTags?.Select(x => x.NormalizeTag()).ToArray() ?? []
            };

            var result = await filterService.ListAsync(filter, tagFilters);

            return Ok(
                result.Cast(ToDisplayItemResponseModel)
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
                result.Cast(ToDisplayItemResponseModel)
            );
        }


        private DisplayItemResponseModel ToDisplayItemResponseModel(DisplayItemDto dto) //TODO: to helper
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
