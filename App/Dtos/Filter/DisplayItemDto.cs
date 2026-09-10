using App.Enums;

namespace App.Dtos.Filter
{
    public record DisplayItemDto
    {
        public DisplayItemType Type { get; set; }
        public int Id { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime ImportTime { get; set; }

        public string? Title { get; set; }  // optional for groups
        public int? Count { get; set; }     // optional for groups
    }
}
