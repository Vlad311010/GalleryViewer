namespace App.Models
{
    public record TagFilters
    {
        public string[] Tags { get; init; } = [];
        public string[] ExcludeTags { get; init; } = [];
    };
}
