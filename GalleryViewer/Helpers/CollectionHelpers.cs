namespace GalleryViewer.Helpers
{
    internal static class CollectionHelpers
    {
        internal static T GetItemCircularly<T>(this IReadOnlyList<T> items, int index)
        {
            ArgumentNullException.ThrowIfNull(items);

            index = index % items.Count;
            if (index < 0)
            {
                index += items.Count;
            }

            return items[index];
        }
    }
}
