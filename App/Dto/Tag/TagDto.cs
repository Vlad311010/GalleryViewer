namespace App.Dto.Tag
{
    public record TagDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int CategoryId { get; set; }

        public int? CanonicalId { get; set; }

        public bool IsCanonocal => !CanonicalId.HasValue;
    }
}
