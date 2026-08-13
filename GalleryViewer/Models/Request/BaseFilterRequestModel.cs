namespace GalleryViewer.Models.Request
{
    public record BaseFilterRequestModel
    {
        // public int GalleryId { get; init; }

        public int Skip { get; init; }
        public int Take { get; init; }
    }
}
