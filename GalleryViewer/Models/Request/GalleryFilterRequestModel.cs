namespace GalleryViewer.Models.Request
{
    public record GalleryFilterRequestModel : PaginationRequestModel
    {
        public IEnumerable<string>? Tags { get; init; }
        public IEnumerable<string>? ExcludeTags { get; init; }
    }
}
