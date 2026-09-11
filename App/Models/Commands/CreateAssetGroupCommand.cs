namespace App.Models.Commands
{
    public record CreateAssetGroupCommand(
        int GalleryId,
        string? GroupName,
        string? PhysicalPath,
        DateTime? CreationTimeOverride = null
    );
}
