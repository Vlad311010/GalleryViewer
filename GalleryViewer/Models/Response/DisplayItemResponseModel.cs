using App.Enum;

namespace GalleryViewer.Models.Response
{
    public record DisplayItemResponseModel
    {
        public DisplayItemType Type { get; set; }
        public int Id { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime ImportTime { get; set; }

        public string? Title { get; set; }
        public int? Count { get; set; }
    }
}
