namespace App.Dtos.Tag
{
    public record TagDtoInfo
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string Category { get; init; } = null!;
        public int Occurrences { get; init; }
        public int? CanonicalId { get; init; }
        public string? CanonicalName { get; init; }

        public bool IsCanonical => !CanonicalId.HasValue;
    }
}
