namespace App.Models.Dtos.Tag
{
    public record TagDto
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public int CategoryId { get; init; }

        public int? CanonicalId { get; init; }

        public bool IsCanonocal => !CanonicalId.HasValue;
    }
}
