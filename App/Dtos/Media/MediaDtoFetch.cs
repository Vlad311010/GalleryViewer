using App.Enums;

namespace App.Dtos.Media
{
    public record MediaDtoFetch(
        DisplayItemType ItemType,
        int ItemId
    );
}
