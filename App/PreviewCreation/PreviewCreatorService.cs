using App.Settings;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace App.PreviewCreation
{
    public class PreviewCreatorService
    {
        PreviewSettings settings;

        public PreviewCreatorService(IOptions<PreviewSettings> settings)
        {
            ArgumentNullException.ThrowIfNull(settings.Value);

            this.settings = settings.Value;
        }


        public async Task<string> CreatePreviewAsync(string galleryRoot, string source)
        {
            using var image = await Image.LoadAsync(source);

            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Size = new Size(settings.Width, 0)
                })
            );


            string destination = GetPreviewPath(galleryRoot, source);
            WebpEncoder encoder = new WebpEncoder
            {
                Quality = settings.Quality
            };


            Directory.CreateDirectory(Path.GetDirectoryName(destination)!); // make sure directorie exsist
            await image.SaveAsWebpAsync(destination, encoder);

            return destination;
        }

        public string GetPreviewPath(string galleryRoot, string sourcePath)
        {
            string sourceRelativePath = Path.GetRelativePath(galleryRoot, sourcePath);
            string? subDirectory = Path.GetDirectoryName(sourceRelativePath);

            string directoryPrefix = string.IsNullOrWhiteSpace(subDirectory) ? string.Empty : $"_{subDirectory}_";
            string previewFileName = $"{settings.Prefix}{directoryPrefix}{Path.GetFileNameWithoutExtension(sourcePath)}{PreviewSettings.WebpSufix}";

            return Path.Combine(settings.PreviewFolder, previewFileName); // Maybe exist issue if file located in root folder TODO: doble check/fix
        }
    }
}
