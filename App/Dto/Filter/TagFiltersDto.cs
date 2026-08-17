namespace App.Dto.Filter
{
    public record TagFiltersDto
    {
        public string[] Tags { get; init; } = [];
        public string[] ExcludeTags { get; init; } = [];
    };
}
