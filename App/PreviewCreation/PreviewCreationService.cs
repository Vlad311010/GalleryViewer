using App.Exceptions;
using App.Extensions;
using App.Interfaces.Services;
using App.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Enums;
using Shared.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace App.PreviewCreation
{
    public class PreviewCreationService : IPreviewCreationService
    {
        private const string PreviewFileExtension = ".webp";
        private readonly PreviewSettings settings;
        private readonly ILogger<PreviewCreationService> logger;

        public PreviewCreationService(IOptions<PreviewSettings> settings, ILogger<PreviewCreationService> logger)
        {
            ArgumentNullException.ThrowIfNull(settings.Value);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings.Value;
            this.logger = logger;
        }


        public async Task<string> StageCreatePreviewAsync(string galleryRoot, string assetDataRef)
        {
            string mimeType = assetDataRef.ToMimeType();
            string mediaType = mimeType.Split('/')[0];
            logger.Debug(
                "Creating {MediaType} preview for {AssetSource}", ApplicationArea.Service,
                mediaType,
                assetDataRef
            );

            string preview;
            switch (mediaType)
            {
                case "image":
                    preview = await GeneratePreviewAsync(galleryRoot, assetDataRef);
                    break;
                case "video":
                    using (Stream previewData = VideoFrameExtractor.GetFrame(assetDataRef, TimeSpan.FromSeconds(1)))
                    {
                        preview = await GeneratePreviewAsync(galleryRoot, assetDataRef, previewData);
                    }
                    break;
                default:
                    throw new NotSupportedMimeTypeException(mimeType);
            }

            logger.Debug(
                "Preview created for {AssetSource} at {PreviewPath}", ApplicationArea.Service,
                assetDataRef,
                preview
            );

            return preview;
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

        private string ConstructPreviewPath(string galleryRoot, string sourcePath)
        {
            string sourceRelativePath = Path.GetRelativePath(galleryRoot, sourcePath);
            string? subDirectory = Path.GetDirectoryName(sourceRelativePath);

            string directoryPrefix = string.IsNullOrWhiteSpace(subDirectory) ? string.Empty : $"_{subDirectory}_";
            string previewFileName = $"{settings.Prefix}{directoryPrefix}{Path.GetFileNameWithoutExtension(sourcePath)}{PreviewFileExtension}";

            return Path.Combine(settings.PreviewFolder, previewFileName);
        }
    }
}
