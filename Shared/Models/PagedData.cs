namespace Shared.Models
{
    public record PagedData<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => Skip > 0;
        public bool HasNext => Skip + Items.Count() >= TotalCount;

        public PagedData(IEnumerable<T> items, int skip, int take, int totalCount)
        {
            Items = items;
            Skip = skip;
            Take = take;
            TotalCount = totalCount;
        }


        public PagedData<T2> Cast<T2>(Func<T, T2> transformer)
        {
            return new PagedData<T2>(
                [.. this.Items.Select(transformer)],
                this.Skip,
                this.Take,
                this.TotalCount
            );
        }

    }
}
