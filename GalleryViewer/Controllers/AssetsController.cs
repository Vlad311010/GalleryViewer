using App.Interfaces.Services;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Tag;
using GalleryViewer.Helpers;
using GalleryViewer.Models;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class AssetsController(IAssetService assetsService) : BaseController
    {
        [HttpGet("{assetId}")]
        [EndpointName("groupAssetInfo")]
        [ProducesResponseType<AssetGroupPositionResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetGroupPosition([FromRoute] int assetId)
        {
            AssetGroupInfoDto? assetGroupInfo = await assetsService.GetAssetGroupInfo(new(assetId));
            if (assetGroupInfo == null)
            {
                return ProblemDetailsBuilder.NotFoundProblem("Asset not found").AsObjectResult();
            }

            int? prevAssetId = assetGroupInfo.GroupId.HasValue ? assetGroupInfo.groupAssets.GetItemCircularly(assetGroupInfo.GroupPosition!.Value - 1) : null;
            int? nextAssetId = assetGroupInfo.GroupId.HasValue ? assetGroupInfo.groupAssets.GetItemCircularly(assetGroupInfo.GroupPosition!.Value + 1) : null;
            return Ok(new AssetGroupPositionResponseModel(
                assetGroupInfo.AssetId,
                assetGroupInfo.GroupId.HasValue,
                assetGroupInfo.GroupId,
                prevAssetId,
                nextAssetId
            ));
        }


        [HttpGet("{assetId}/tags")]
        [EndpointName("assetTags")]
        [ProducesResponseType<AssetTagsResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssetTags([FromRoute] int assetId)
        {
            AssetTagsDto tags = await assetsService.GetAssetTags(new(assetId));

            return Ok(new AssetTagsResponseModel(tags.Tags));
        }

        [HttpPost("{assetId}/tags")]
        [EndpointName("addAssetTags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<InvalidTagsProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddTags([FromRoute] int assetId, [FromBody] IEnumerable<string> tags)
        {
            AssetAddTagsCommand command = new(assetId, [.. tags.Select(x => x.NormalizeTag())]);
            await assetsService.AddTags(command);

            return NoContent();
        }

        [HttpDelete("{assetId}/tags/{tag}")]
        [EndpointName("removeAssetTag")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTag([FromRoute] int assetId, [FromRoute] string tag)
        {
            AssetRemoveTagCommand command = new(assetId, tag.NormalizeTag());
            await assetsService.RemoveTag(command);

            return NoContent();
        }

    }
}
