using App.Enum;

namespace App.Dto
{
    public record MediaFetchDto(
        DisplayItemType ItemType,
        int ItemId
    );
}
