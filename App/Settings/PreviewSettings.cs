namespace App.Settings
{
    public record PreviewSettings
    {
        public const string SectionName = nameof(PreviewSettings);
        public const string WebpSufix = ".webp";

        public int Width { get; set; }
        public int Quality { get; set; }
        public required string PreviewFolder { get; set; }
        public string Prefix { get; set; } = "preview_";
    }
}
