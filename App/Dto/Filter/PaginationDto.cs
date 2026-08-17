namespace App.Dto.Filter
{
    public record PaginationDto
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
