using App;
using GalleryViewer.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    public class TagsController(TagsServices tagsServices) : BaseController
    {
        [HttpGet("search")]
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
    }
}
