namespace App.Dto.Group
{
    public record AssetGroupDtoCreate(
        int GalleryId,
        string? GroupName,
        string? PhysicalPath,
        DateTime? CreationTimeOverride = null
    );
}
