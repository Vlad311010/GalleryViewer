using Shared.Models;

namespace App.Commands
{
    public record SetAssetsPositionsCommand(int GroupId, IEnumerable<AssetPosition> Positions);
}
