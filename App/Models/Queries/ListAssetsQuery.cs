namespace App.Models.Queries
{
    public record ListAssetsQuery(string GalleryName, Pagination Pagination, TagFilters TagFilters);
}
