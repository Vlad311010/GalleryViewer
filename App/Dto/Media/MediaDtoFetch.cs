using App.Enum;

namespace App.Dto.Media
{
    public record MediaDtoFetch(
        DisplayItemType ItemType,
        int ItemId
    );
}
