using App.Enum;
using System.ComponentModel.DataAnnotations;

namespace GalleryViewer.Models.Response
{
    public record DisplayItemResponseModel
    {
        [Required]
        public DisplayItemType Type { get; init; }

        [Required]
        public int Id { get; init; }

        [Required]
        public DateTime CreationTime { get; init; }

        [Required]
        public DateTime ImportTime { get; init; }

        public string? Title { get; init; }
        public int? Count { get; init; }
    }
}
