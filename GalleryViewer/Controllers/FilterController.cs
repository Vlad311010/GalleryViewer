using App;
using App.Dto.Filter;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace GalleryViewer.Controllers
{

    public class FilterController(FilterService filterService) : BaseController
    {
        [HttpGet]
        [ProducesResponseType<PagedData<DisplayItemResponseModel>>(StatusCodes.Status200OK)]
        [EndpointName("filter")]
        public async Task<IActionResult> Filter([FromQuery] FilterRequestModel request)
        {
            FilterDto filter = new() { Skip = request.Skip, Take = request.Take };
            var result = await filterService.ListAsync(filter);

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
