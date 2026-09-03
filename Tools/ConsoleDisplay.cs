using Tools.Sync;

namespace Tools
{
    internal static class ConsoleDisplay
    {
        public static void Display(string str)
        {
            Console.WriteLine(str);
        }

        public static void DisplayError(string str)
        {
            Console.WriteLine($"Error: {str}");
        }

        public static void DisplaySyncState(SyncStateData state)
        {
            Console.Clear();

            Console.WriteLine($"Gallery: {state.CurrentGallery ?? "-"}".PadRight(100));
            Console.WriteLine($"Group:   {state.CurrentGroup ?? "-"}".PadRight(100));
            Console.WriteLine($"Asset:   {state.CurrentAsset ?? "-"}".PadRight(100));
            Console.WriteLine();
            Console.WriteLine($"Galleries Processed: {state.GalleriesSynchronized}".PadRight(100));
            Console.WriteLine();
            Console.WriteLine($"Assets Created:   {state.CreatedAssets}".PadRight(100));
            Console.WriteLine($"Assets Skipped:   {state.SkippedAssets}".PadRight(100));
            Console.WriteLine($"Assets Deleted:   {state.DeletedAssets}".PadRight(100));
            Console.WriteLine();
            Console.WriteLine($"Groups Created:    {state.CreatedGroups}".PadRight(100));
        }
    }
}
