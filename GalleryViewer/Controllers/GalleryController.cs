using App.Interfaces.Services;
using App.Models.Dtos.Gallery;
using App.Models.Queries;
using GalleryViewer.Helpers;
using GalleryViewer.Mappers;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class GalleryController(IGalleriesService galleriesService) : BaseController
    {
        [HttpGet]
        [EndpointName("galleries")]
        [ProducesResponseType<IEnumerable<GalleryResponseModel>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            IEnumerable<GalleryDto> galleries = await galleriesService.ListAsync();

            return Ok(
                galleries.Select(x => x.ToGalleryResponseModel())
            );
        }

        [HttpGet("{name}")]
        [EndpointName("galleryByName")]
        [ProducesResponseType<GalleryResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName([FromRoute] string name)
        {
            GalleryDto? gallery = await galleriesService.GetByNameAsync(new GalleryByNameQuery(name));
            if (gallery == null)
            {
                return ProblemDetailsBuilder.NotFoundProblem($"Gallery {name} not found").AsObjectResult();
            }

            return Ok(
                gallery.ToGalleryResponseModel()
            );
        }
    }
}
