using App;
using App.Dto;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    public class MediaController(MediaService mediaService) : BaseController
    {

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsset([FromRoute] int id)
        {
            var requestDto = new AssetMediaDtoFetch(id);
            var response = await mediaService.GetAssetMediaAsync(requestDto);

            return File(response.MediaStream, response.MimeType, true);
        }

        [HttpGet("preview/{id}")]
        public async Task<IActionResult> GetPreview([FromRoute] int id)
        {
            var requestDto = new MediaDtoFetch(App.Enum.DisplayItemType.Asset, id);
            var response = await mediaService.GetAssetPreviewAsync(requestDto);

            return File(response.MediaStream, response.MimeType, true);
        }
    }
}
