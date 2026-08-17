namespace GalleryViewer.Models.Request
{
    public record GalleryFilterRequestModel : BaseFilterRequestModel
    {
        public IEnumerable<string>? Tags { get; init; }
        public IEnumerable<string>? ExcludeTags { get; init; }
    }
}
