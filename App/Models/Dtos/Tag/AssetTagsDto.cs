namespace App.Models.Dtos.Tag
{
    public record AssetTagsDto(Dictionary<string, TagDtoInfo[]> Tags);
}
