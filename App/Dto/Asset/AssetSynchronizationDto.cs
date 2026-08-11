using App.Enum;

namespace App.Dto.Asset
{
    public record AssetSynchronizationDto(SyncMismatchType Type, int? id, string RelativePath);
}
