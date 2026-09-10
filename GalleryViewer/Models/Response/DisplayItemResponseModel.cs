using App.Enums;

namespace GalleryViewer.Models.Response
{
    public record DisplayItemResponseModel
    {
        public DisplayItemType Type { get; init; }
        public int Id { get; init; }
        public DateTime CreationTime { get; init; }
        public DateTime ImportTime { get; init; }
        public string? Title { get; init; }
        public int? Count { get; init; }
    }
}
