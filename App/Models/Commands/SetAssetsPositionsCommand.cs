using Shared.Models;

namespace App.Models.Commands
{
    public record SetAssetsPositionsCommand(int GroupId, IEnumerable<AssetPosition> Positions);
}
