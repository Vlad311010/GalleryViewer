namespace App.Models.Commands
{
    public record AssetAddTagsCommand(int AssetId, string[] Tags);
}
