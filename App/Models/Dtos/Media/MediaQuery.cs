using App.Enums;

namespace App.Models.Dtos.Media
{
    public record MediaQuery(
        DisplayItemType ItemType,
        int ItemId
    );
}
