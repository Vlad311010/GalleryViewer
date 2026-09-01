using App.Exceptions;
using App.Extensions;
using App.Settings;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace App.PreviewCreation
{
    public class PreviewCreatorService
    {
        public const string PreviewFileExtension = ".webp";
        public const string PreviewFileMimeType = "image/webp";
        PreviewSettings settings;

        public PreviewCreatorService(IOptions<PreviewSettings> settings)
        {
            ArgumentNullException.ThrowIfNull(settings.Value);

            this.settings = settings.Value;
        }


        public async Task<string> CreatePreviewAsync(string galleryRoot, string source)
        {
            string mimeType = source.ToMimeType();
            string mediaType = mimeType.Split('/')[0];
            switch (mediaType)
            {
                case "image":
                    return await GeneratePreviewAsync(galleryRoot, source);
                case "video":
                    using (Stream previewData = VideoFrameExtractor.GetFrame(source, TimeSpan.FromSeconds(1)))
                    {
                        return await GeneratePreviewAsync(galleryRoot, source, previewData);
                    }
                default:
                    throw new NotSupportedMimeTypeException(mimeType);
            }
        }

        private async Task<string> GeneratePreviewAsync(string galleryRoot, string assetSource)
        {
            using var image = await Image.LoadAsync(assetSource);
            return await GeneratePreviewAsync(galleryRoot, assetSource, image);
        }

        private async Task<string> GeneratePreviewAsync(string galleryRoot, string assetSource, Stream previewData)
        {
            using var image = await Image.LoadAsync(previewData);
            return await GeneratePreviewAsync(galleryRoot, assetSource, image);
        }

        private async Task<string> GeneratePreviewAsync(string galleryRoot, string assetSource, Image image)
        {
            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Size = new Size(settings.Width, 0)
                })
            );


            string destination = ConstructPreviewPath(galleryRoot, assetSource);
            WebpEncoder encoder = new WebpEncoder
            {
                Quality = settings.Quality
            };


            Directory.CreateDirectory(Path.GetDirectoryName(destination)!); // make sure directorie exsist
            await image.SaveAsWebpAsync(destination, encoder);

            return destination;
        }

        public string ConstructPreviewPath(string galleryRoot, string sourcePath)
        {
            string sourceRelativePath = Path.GetRelativePath(galleryRoot, sourcePath);
            string? subDirectory = Path.GetDirectoryName(sourceRelativePath);

            string directoryPrefix = string.IsNullOrWhiteSpace(subDirectory) ? string.Empty : $"_{subDirectory}_";
            string previewFileName = $"{settings.Prefix}{directoryPrefix}{Path.GetFileNameWithoutExtension(sourcePath)}{PreviewFileExtension}";

            return Path.Combine(settings.PreviewFolder, previewFileName); // Maybe exist issue if file located in root folder TODO: doble check/fix
        }
    }
}
