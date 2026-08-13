namespace GalleryViewer.Models.Request
{
    public record TagSearchResponseModel
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Category { get; init; } = null!;

        public int Occurrences { get; init; }

        public bool IsCanonical { get; init; }

        public string? CanonicalName { get; init; }
    }
}
