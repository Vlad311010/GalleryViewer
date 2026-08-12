namespace Shared.Models
{
    public record PagedData<T>
    {
        public IEnumerable<T> Items { get; init; }
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public int PagesCount { get; init; }

        public bool HasPrevious => Skip > 0;
        public bool HasNext => Skip + Items.Count() >= TotalCount;

        public PagedData(IEnumerable<T> items, int skip, int take, int totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(take, 0);

            Items = items;
            Skip = skip;
            Take = take;
            TotalCount = totalCount;
            PagesCount = (TotalCount + Take - 1) / Take;
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
