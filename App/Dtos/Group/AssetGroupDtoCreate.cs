namespace App.Dtos.Group
{
    public record AssetGroupDtoCreate(
        int GalleryId,
        string? GroupName,
        string? PhysicalPath,
        DateTime? CreationTimeOverride = null
    );
}
