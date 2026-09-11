using App.Enums;

namespace App.Models.Dtos.Asset
{
    public record AssetSynchronizationDto(SyncMismatchType Type, int? id, string RelativePath);
}
