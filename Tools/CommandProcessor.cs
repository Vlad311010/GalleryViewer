using CommandLine;
using System.Text.Json;
using Tools.Models;

namespace Tools
{
    internal static class CommandProcessor
    {
        enum Commands
        {
            config,
            single
        }

        [Verb("sync", HelpText = "Synchronize galleries.")]
        private class SyncOptions
        {
            [Value(0, MetaName = "path", Required = true, HelpText = "Path to config .json file")]
            public string ConfigPath { get; set; } = "";
        }

        [Verb("help", isDefault: true, aliases: ["-h"], HelpText = "Displays help")]
        private class HelpOptions
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
            public Commands Command { get; set; }
        }

        public static async Task<int> Run(string[] args, CompositionRoot compositionRoot)
        {
            return await Parser.Default
                .ParseArguments<
                    SyncOptions,
                    HelpOptions>(args)
                .MapResult(
                    async (SyncOptions o) => await RunSync(o, compositionRoot),
                    errors => Task.FromResult(1));
        }

        private static async Task<int> RunSync(SyncOptions options, CompositionRoot compositionRoot)
        {
            Loggining.Log("Executin: Sync");


            if (!File.Exists(options.ConfigPath))
            {
                Loggining.Error("File not found");
                return 1;
            }
            FileStream fileStream = new FileStream(options.ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            GallerySyncData? data = JsonDocument.Parse(fileStream).Deserialize<GallerySyncData>();
            if (data == null)
            {
                Loggining.Error("Failed to parse config file");
                return 1;
            }


            GallerySync gallerySync = compositionRoot.CreateGallerySync();
            await gallerySync.SyncronizeGalleryAsync(data);

            return 0;
        }

        private static int RunHelp(HelpOptions options)
        {
            Loggining.Log(nameof(RunHelp));
            return 0;
        }
    }
}
