using System.ComponentModel.DataAnnotations;

namespace GalleryViewer.Models.Response
{
    public record TagSearchResponseModel
    {
        [Required]
        public int Id { get; init; }

        [Required]
        public string Name { get; init; } = null!;

        [Required]
        public string Category { get; init; } = null!;

        [Required]
        public int Occurrences { get; init; }

        [Required]
        public bool IsCanonical { get; init; }

        public string? CanonicalName { get; init; }
    }
}
