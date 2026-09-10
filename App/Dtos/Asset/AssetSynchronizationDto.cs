using App.Enums;

namespace App.Dtos.Asset
{
    public record AssetSynchronizationDto(SyncMismatchType Type, int? id, string RelativePath);
}
