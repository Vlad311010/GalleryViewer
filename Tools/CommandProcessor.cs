using CommandLine;
using System.Text.Json;
using Tools.Display;
using Tools.Models;
using Tools.Sync;

namespace Tools
{
    internal static class CommandProcessor
    {

        [Verb("sync", aliases: ["s"], HelpText = "Synchronize galleries.")]
        private class SyncOptions
        {
            [Value(0, MetaName = "path", Required = true, HelpText = "Path to config .json file")]
            public string ConfigPath { get; set; } = "";
        }

        [Verb("preview", aliases: ["p"], HelpText = "Generates preview for given asset. Overrides existing one.")]
        private class PreviewOptions
        {
            [Value(0, MetaName = "asset", Required = true, HelpText = "Asset id")]
            public int AssetId { get; set; }
        }

        public static async Task<int> Run(string[] args, CompositionRoot compositionRoot)
        {
            if (args.Length == 0)
            {
                var parser = new Parser(with =>
                {
                    with.HelpWriter = Console.Out;
                });

                parser.ParseArguments<
                    SyncOptions,
                    PreviewOptions>(new[] { "--help" });

                return 0;
            }

            return await Parser.Default
                .ParseArguments<
                    SyncOptions,
                    PreviewOptions>(args)
                .MapResult(
                    async (SyncOptions o) => await RunSync(o, compositionRoot),
                    async (PreviewOptions o) => await RunPreview(o, compositionRoot),

                    errors => Task.FromResult(1));
        }

        private static async Task<int> RunSync(SyncOptions options, CompositionRoot compositionRoot)
        {
            ConsoleDisplay.Display("Executing: Sync");

            if (!File.Exists(options.ConfigPath))
            {
                ConsoleDisplay.DisplayError("File not found");
                return 1;
            }
            FileStream fileStream = new FileStream(options.ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            GallerySyncData? data = JsonDocument.Parse(fileStream).Deserialize<GallerySyncData>();
            if (data == null)
            {
                ConsoleDisplay.DisplayError("Failed to parse config file");
                return 1;
            }


            var displayTask = Task.Run(() => LiveSyncStatusDisplay.Start());

            GallerySync gallerySync = compositionRoot.CreateGallerySync();

            gallerySync.OnProgressUpdated += (_, state) =>
            {
                LiveSyncStatusDisplay.DisplaySyncState(state);
            };

            await gallerySync.Syncronize(data);

            LiveSyncStatusDisplay.Stop();
            return 0;
        }

        private static async Task<int> RunPreview(PreviewOptions options, CompositionRoot compositionRoot)
        {
            ConsoleDisplay.Display(nameof(RunPreview));
            return 0;
        }
    }
}
