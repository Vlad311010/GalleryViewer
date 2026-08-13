using App;
using App.Dto.Filter;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{

    public class AssetBrowserController(FilterService filterService, AssetsService assetsService) : BaseController
    {
        [HttpGet()]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("listItems")]
        public async Task<IActionResult> ListGalleryItems([FromQuery] BaseFilterRequestModel request)
        {
            BaseFilterDto filter = new() { Skip = request.Skip, Take = request.Take };
            var result = await filterService.ListAsync(filter);

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
            BaseFilterDto filter = new() { Skip = filterRequest.Skip, Take = filterRequest.Take };
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
