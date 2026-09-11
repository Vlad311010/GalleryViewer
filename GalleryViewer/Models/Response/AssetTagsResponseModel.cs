using App.Models.Dtos.Tag;

namespace GalleryViewer.Models.Response
{
    public record AssetTagsResponseModel(Dictionary<string, TagDtoInfo[]> Tags);
}
