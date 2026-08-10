using App.Enum;

namespace App.Dto
{
    public record AssetSynchronizationDto(SyncMismatchType Type, int? id, string RelativePath);
}
