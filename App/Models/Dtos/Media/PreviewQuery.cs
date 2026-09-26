using App.Enums;

namespace App.Models.Dtos.Media
{
    public record PreviewQuery(
        DisplayItemType ItemType,
        int ItemId
    );
}
