using App.Enum;

namespace App.Dto
{
    public record MediaDtoFetch(
        DisplayItemType ItemType,
        int ItemId
    );
}
