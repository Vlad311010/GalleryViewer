using App.Enums;
using App.Interfaces.Services;
using App.Models.Dtos.Media;
using App.Models.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{

    public class MediaController(IMediaService mediaService) : BaseController
    {
        [Produces("application/octet-stream")]
        [HttpGet("asset/{id}")]
        [EndpointName("asset")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsset([FromRoute] int id)
        {
            var query = new AssetQuery(id);
            var response = await mediaService.GetAssetMediaAsync(query);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("application/octet-stream")]
        [HttpGet("asset/preview/{id}")]
        [EndpointName("assetPreview")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetPreview([FromRoute] int id)
        {
            var query = new MediaQuery(DisplayItemType.Asset, id);
            var response = await mediaService.GetPreviewAsync(query);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("application/octet-stream")]
        [HttpGet("group/preview/{id}")]
        [EndpointName("groupPreview")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupPreview([FromRoute] int id)
        {
            var query = new MediaQuery(DisplayItemType.Group, id);
            var response = await mediaService.GetPreviewAsync(query);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("text/plain")]
        [HttpGet("asset/{id}/mime-type")]
        [EndpointName("assetMimeType")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMimeType([FromRoute] int id)
        {
            var query = new AssetQuery(id);
            var mimeType = await mediaService.GetAssetMimeType(query);

            return Ok(mimeType);
        }
    }
}
