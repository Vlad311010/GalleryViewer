using App;
using App.Dto.Group;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    [Produces("application/json")]
    public class GroupsController(GroupsService groupsService) : BaseController
    {
        [HttpGet("{id}")]
        [EndpointName("groupDetails")]
        [ProducesResponseType<AssetGroupResponseModel>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            AssetGroupDtoWithAssetPositions group = await groupsService.GetByIdAsync(id);

            return Ok(new AssetGroupResponseModel(
                group.Id,
                group.GalleryId,
                group.CoverAssetIdx,
                string.IsNullOrWhiteSpace(group.PhysicalPath),
                group.Positions.Count(),
                group.Positions,
                group.Title,
                group.PhysicalPath
            ));
        }

        public record AssetGroupResponseModel(
            int Id,
            int GalleryId,
            int CoverAssetPosition,
            bool IsEditable,
            int AssetsCount,
            IEnumerable<AssetPosition> Positions,
            string? Title,
            string? PhysicalPath
        );
    }
}
