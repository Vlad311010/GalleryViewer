namespace App.Dto.Tag
{
    public record TagDtoCreate
    {
        public string Name { get; set; } = null!;

        public string Category { get; set; }

        public int? CanonicalId { get; set; }
    }
}
