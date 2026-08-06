using App;
using App.Dto;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{

    public class FilterController(FilterService filterService) : BaseController
    {
        [HttpGet("{page}")]
        public async Task<IActionResult> Filter(int page = 1)
        {
            FilterDto filter = new() { Skip = 5 * (page - 1), Take = 20 };
            var result = await filterService.ListAsync(filter);

            return Ok(result);
        }


    }
}
