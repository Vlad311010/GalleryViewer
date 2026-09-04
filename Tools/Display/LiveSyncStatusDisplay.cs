using Spectre.Console;
using Spectre.Console.Rendering;
using Tools.Sync;

namespace Tools.Display
{
    public static class LiveSyncStatusDisplay
    {
        private static readonly object SyncLock = new();

        private static LiveDisplayContext? _context;

        public static void Start()
        {
            var states = new SyncState();

            AnsiConsole.Live(CreateDisplay(states))
                .Overflow(VerticalOverflow.Visible)
                .Start(ctx =>
                {
                    _context = ctx;

                    while (_context != null)
                    {
                    }
                });
        }

        private static IRenderable CreateDisplay(SyncState state)
        {
            var rows = new List<IRenderable>();

            foreach (var galleryName in state.GalleriesToProcess)
            {
                rows.Add(CreatePanel(state, galleryName));
                rows.Add(new Text(""));
            }

            return new Rows(rows);
        }

        public static void Stop()
        {
            lock (SyncLock)
            {
                _context = null;
            }
        }

        public static void DisplaySyncState(SyncState state)
        {
            lock (SyncLock)
            {
                _context?.UpdateTarget(CreateDisplay(state));
                _context?.Refresh();
            }
        }

        private static IRenderable CreatePanel(SyncState syncState, string galleryName)
        {
            if (!syncState.Keys.Contains(galleryName))
            {
                return new Panel(
                    new Rows(
                        new Text(""),
                        new Markup("[grey]Waiting for sync to start...[/]")
                    ))
                    .Header($"[bold]Gallery: {Markup.Escape(galleryName)} [/]")
                    .Border(BoxBorder.Rounded);
            }

            var galleryState = syncState[galleryName];

            var processed =
                galleryState.CreatedAssets +
                galleryState.SkippedAssets;

            var total = galleryState.TotalFilesToProcess;

            var percentage = total > 0
                ? Math.Min(100, (double)processed / total * 100)
                : 0;

            var progressBar = CreateProgressBar(processed, total);

            var lastGroup = string.IsNullOrWhiteSpace(galleryState.LastGroup)
                ? "-"
                : galleryState.LastGroup;

            var lastAsset = string.IsNullOrWhiteSpace(galleryState.LastAsset)
                ? "-"
                : galleryState.LastAsset;

            var breakdown = new BreakdownChart()
                .Width(60)
                .FullSize()
                .AddItem("Created", galleryState.CreatedAssets, Color.Green)
                .AddItem("Skipped", galleryState.SkippedAssets, Color.Yellow)
                .AddItem("Deleted", galleryState.DeletedAssets, Color.Red);

            return new Panel(
                new Rows(
                    new Markup(
                        $"[bold]Group:[/] {Markup.Escape(lastGroup)}"),

                    new Markup(
                        $"[bold]Asset:[/] {Markup.Escape(lastAsset)}"),

                    new Text(""),

                    new Markup(
                        $"{processed:N0} / {total:N0} ({percentage:F1}%)"),

                    progressBar,

                    new Text(""),

                    breakdown))
                .Header($"[bold]Gallery: {Markup.Escape(galleryName)} [/]")
                .Border(BoxBorder.Rounded);
        }

        private static IRenderable CreateProgressBar(int processed, int total, int width = 60)
        {
            var percentage = total > 0
                ? Math.Clamp((double)processed / total, 0, 1)
                : 0;

            var filled = (int)Math.Round(width * percentage);
            var remaining = width - filled;

            var percentText = $"{percentage:P0}";

            return new Markup(
                $"[green]{new string('─', filled)}[/]" +
                $"[grey]{new string('─', remaining)}[/] " +
                $"[bold]{percentText}[/]");
        }
    }
}
