namespace GalleryViewer.Models.Request
{
    public record BaseFilterRequestModel
    {
        public int Skip { get; init; }
        public int Take { get; init; }
    }
}
