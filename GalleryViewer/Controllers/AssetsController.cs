using App;
using App.Dto.Tag;
using GalleryViewer.Helpers;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class AssetsController(AssetsService assetsService) : BaseController
    {
        [HttpGet("{assetId}/tags")]
        [EndpointName("assetTags")]
        [ProducesResponseType<AssetTagsResponseModel>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetTags([FromRoute] int assetId)
        {
            AssetTagsDto tags = await assetsService.GetAssetTags(assetId);

            return Ok(new AssetTagsResponseModel(tags.Tags));
        }

        [HttpPost("{assetId}/tags")]
        [EndpointName("addAssetTags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTags([FromRoute] int assetId, [FromBody] IEnumerable<string> tags)
        {

            await assetsService.AddTags(assetId, [.. tags.Select(x => x.NormalizeTag())]);

            return NoContent();
        }

        [HttpDelete("{assetId}/tags/{tag}")]
        [EndpointName("removeAssetTag")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTag([FromRoute] int assetId, [FromRoute] string tag)
        {
            await assetsService.RemoveTag(assetId, tag.NormalizeTag());

            return NoContent();
        }

    }
}
