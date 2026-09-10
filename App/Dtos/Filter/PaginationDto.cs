namespace App.Dtos.Filter
{
    public record PaginationDto
    {
        public int Skip { get; init; }
        public int Take { get; init; }
    }
}
