using App.Dto.Group;
using App.Interfaces.Services;
using GalleryViewer.Models.Request;
using GalleryViewer.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class GroupsController(IGroupService groupsService) : BaseController
    {
        [HttpGet("{id}")]
        [EndpointName("groupDetails")]
        [ProducesResponseType<AssetGroupResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            AssetGroupDtoWithAssetPositions group = await groupsService.GetByIdAsync(id);

            return Ok(new AssetGroupResponseModel(
                group.Id,
                group.GalleryId,
                group.CoverAssetIdx,
                string.IsNullOrWhiteSpace(group.PhysicalPath),
                [.. group.Positions],
                group.Title
            ));
        }

        [HttpPost("{id}/positions")]
        [EndpointName("groupSetPositons")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetAssetPositions([FromRoute] int id, [FromBody] SetAssetPositionsRequestModel request)
        {
            await groupsService.SetPositionsAsync(id, request.Positions);
            return NoContent();
        }

        [HttpPost("{id}/cover")]
        [EndpointName("groupSetCover")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetGroupCoverAsset([FromRoute] int id, [FromBody] SetGroupCoverRequestModel request)
        {
            await groupsService.SetCover(id, request.AssetId);
            return NoContent();
        }
    }
}
