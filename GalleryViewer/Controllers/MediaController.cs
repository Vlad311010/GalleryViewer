using App;
using App.Dto;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    public class MediaController(MediaService mediaService) : BaseController
    {
        [HttpGet("")]
        public async Task<IActionResult> GetPreview()
        {
            var data = new MediaFetchDto(App.Enum.DisplayItemType.Asset, 1);
            var response = await mediaService.GetAssetPreviewAsync(data);

            return File(response.MediaStream, response.MimeType, true);
        }
    }
}
