namespace GalleryViewer.Models.Request
{
    public record FilterRequestModel
    {
        public int Skip { get; init; }
        public int Take { get; init; }
    }
}
