using App;
using App.Dto.Gallery;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    public class GalleryController(GalleriesService galleriesService) : BaseController
    {
        [HttpGet]
        [EndpointName("galleries")]
        [ProducesResponseType<IEnumerable<GalleryResponseModel>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            IEnumerable<GalleryDto> galleries = await galleriesService.ListAsync();

            return Ok(
                galleries.Select(ToGalleryResponseModel)
            );
        }

        [HttpGet("{name}")]
        [EndpointName("galleryByName")]
        [ProducesResponseType<GalleryResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName([FromRoute] string name)
        {
            GalleryDto? gallery = await galleriesService.GetByNameAsync(name);
            if (gallery == null)
            {
                return NotFound();
            }

            return Ok(
                ToGalleryResponseModel(gallery)
            );
        }

        private GalleryResponseModel ToGalleryResponseModel(GalleryDto dto)
        {
            return new(dto.Id, dto.Name, dto.CoverAssetId);
        }
    }
}
