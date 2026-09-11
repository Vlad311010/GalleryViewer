namespace GalleryViewer.Models.Request
{
    public record PaginationRequestModel
    {
        public int Skip { get; init; }
        public int Take { get; init; }
    }
}
