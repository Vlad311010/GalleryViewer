using App.Dto.Media;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{

    public class MediaController(MediaService mediaService) : BaseController
    {
        [Produces("application/octet-stream")]
        [HttpGet("asset/{id}")]
        [EndpointName("asset")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsset([FromRoute] int id)
        {
            var requestDto = new AssetMediaDtoFetch(id);
            var response = await mediaService.GetAssetMediaAsync(requestDto);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("application/octet-stream")]
        [HttpGet("asset/preview/{id}")]
        [EndpointName("assetPreview")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetPreview([FromRoute] int id)
        {
            var requestDto = new MediaDtoFetch(App.Enum.DisplayItemType.Asset, id);
            var response = await mediaService.GetAssetPreviewAsync(requestDto);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("application/octet-stream")]
        [HttpGet("group/preview/{id}")]
        [EndpointName("groupPreview")]
        [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupPreview([FromRoute] int id)
        {
            var requestDto = new MediaDtoFetch(App.Enum.DisplayItemType.Group, id);
            var response = await mediaService.GetAssetPreviewAsync(requestDto);

            return File(response.MediaStream, response.MimeType, true);
        }

        [Produces("text/plain")]
        [HttpGet("asset/{id}/mime-type")]
        [EndpointName("assetMimeType")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMimeType([FromRoute] int id)
        {
            var requestDto = new AssetMediaDtoFetch(id);
            var mimeType = await mediaService.GetAssetMimeType(requestDto);

            return Ok(mimeType);
        }
    }
}
