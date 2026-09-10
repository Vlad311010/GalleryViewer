namespace App.Commands
{
    public record AssetCreateCommand(
        int GalleryId,
        string RelativePath,
        string? PreviewPath,
        int? GroupId,
        int? GroupPosition
    );
}
