using App.Interfaces.Services;
using App.Models.Commands;
using App.Models.Dtos.Group;
using App.Models.Queries;
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
            AssetGroupDtoWithAssetPositions group = await groupsService.GetByIdAsync(new AssetGroupQuery(id));

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
            SetAssetsPositionsCommand command = new(id, request.Positions);
            await groupsService.SetPositionsAsync(command);
            return NoContent();
        }

        [HttpPost("{id}/cover")]
        [EndpointName("groupSetCover")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetGroupCoverAsset([FromRoute] int id, [FromBody] SetGroupCoverRequestModel request)
        {
            SetGroupCoverCommand command = new(id, request.AssetId);
            await groupsService.SetCover(command);
            return NoContent();
        }
    }
}
