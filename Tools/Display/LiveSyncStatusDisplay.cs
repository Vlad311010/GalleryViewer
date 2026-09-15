using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Immutable;
using System.Threading.Channels;
using Tools.Sync;

namespace Tools.Display
{
    public static class LiveSyncStatusDisplay
    {
        private static readonly object SyncLock = new();
        private static readonly ManualResetEventSlim StopEvent = new(false);

        private static LiveDisplayContext? context;

        private static Channel<SyncState> StateChannel = Channel.CreateUnbounded<SyncState>();
        private static Task displayTask = Task.CompletedTask;

        public static void Start()
        {
            var initialState = new SyncState(
                0,
                new Dictionary<string, GallerySyncState>());

            StateChannel = Channel.CreateUnbounded<SyncState>();

            displayTask = Task.Run(() =>
            {
                AnsiConsole.Live(CreateDisplay(initialState))
                    .Overflow(VerticalOverflow.Visible)
                    .Start(ctx =>
                    {
                        while (StateChannel.Reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult())
                        {
                            while (StateChannel.Reader.TryRead(out var state))
                            {
                                ctx.UpdateTarget(CreateDisplay(state));
                                ctx.Refresh();
                            }
                        }
                    });
            });
        }

        private static IRenderable CreateDisplay(SyncState state)
        {
            var rows = new List<IRenderable>();

            foreach (var galleryName in state.Galleries)
            {
                rows.Add(new Text(""));
                rows.Add(CreatePanel(state, galleryName));
            }
            rows.Add(new Text(""));

            return new Rows(rows);
        }

        public async static Task StopAsync()
        {
            StateChannel.Writer.TryComplete();
            await Task.WhenAll(displayTask);
        }

        public static void DisplaySyncState(SyncState state)
        {
            StateChannel.Writer.TryWrite(state);
        }

        private static IRenderable CreatePanel(SyncState syncState, string galleryName)
        {
            if (!syncState.Galleries.Contains(galleryName))
            {
                return new Panel(
                    new Rows(
                        new Text(""),
                        new Markup("[grey]Waiting for sync to start...[/]")
                    ))
                    .Header($"[bold]Gallery: {Markup.Escape(galleryName)} [/]")
                    .Border(BoxBorder.Rounded);


            }

            var galleryState = syncState.State[galleryName];

            var processed =
                galleryState.CreatedAssets +
                galleryState.SkippedAssets;

            double percentage = 0;
            if (galleryState.TotalFilesToProcess.HasValue)
            {
                int total = galleryState.TotalFilesToProcess.Value;
                percentage = total > 0
                    ? Math.Min(100, (double)processed / total)
                    : 0;
            }

            var progressBar = CreateProgressBar(percentage);

            var lastAsset = string.IsNullOrWhiteSpace(galleryState.LastAsset)
                ? "-"
                : galleryState.LastAsset;

            var breakdown = new BreakdownChart()
                .Width(60)
                .FullSize()
                .AddItem("Created", galleryState.CreatedAssets, Color.Green)
                .AddItem("Skipped", galleryState.SkippedAssets, Color.Yellow)
                .AddItem("Deleted", galleryState.DeletedAssets, Color.Red);


            var header = galleryState.ElapsedTime.HasValue
                ? $"[bold] Gallery: {Markup.Escape(galleryName)} | Processing Time: {galleryState.ElapsedTime} [/]"
                : $"[bold] Gallery: {Markup.Escape(galleryName)} [/]";

            return new Panel(
                new Rows(
                    new Markup(
                        $"[bold]Asset:[/] {Markup.Escape(lastAsset)}"),

                    new Text(""),

                    new Markup(galleryState.TotalFilesToProcess.HasValue ? $"{processed:N0} / {galleryState.TotalFilesToProcess:N0} ({percentage * 100:F1}%)" : "???"),

                    progressBar,

                    new Text(""),

                    breakdown))
            {
                Width = Math.Max(Console.WindowWidth, 70)
            }
                .Header(header)
                .Border(BoxBorder.Rounded);

        }

        private static IRenderable CreateProgressBar(double percentage, int width = 60)
        {
            percentage = Math.Clamp(percentage, 0, 1);
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
