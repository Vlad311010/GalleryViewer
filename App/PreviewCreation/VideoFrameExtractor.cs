
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace App.PreviewCreation
{
    internal static class VideoFrameExtractor
    {
        static string outputFile = @"F:\_my\preview.webp";

        public static async Task<string> SaveFrameAsync(string filePath, TimeSpan at)
        {
            const string defaultDispositionKey = "default";

            var mediaInfo = await FFProbe.AnalyseAsync(filePath);

            var defaultVideoStream = mediaInfo.VideoStreams
                .FirstOrDefault(x => x.Disposition.GetValueOrDefault(defaultDispositionKey))
                ?? mediaInfo.VideoStreams.FirstOrDefault();

            if (defaultVideoStream == null)
            {
                throw new Exception("TODO:proper exception handling. Faild to get primary/default videao stream");
            }


            int width = defaultVideoStream.Width;
            int height = defaultVideoStream.Height;

            bool success = await FFMpeg.SnapshotAsync(filePath, outputFile, new System.Drawing.Size(width, height), at);
            if (!success)
            {
                //TODO: exception hanlding
            }


            return outputFile;
        }

        public static MemoryStream GetFrame(string filePath, TimeSpan at)
        {
            var stream = new MemoryStream();

            FFMpegArguments
              .FromFileInput(filePath)
              .OutputToPipe(
                  new StreamPipeSink(stream),
                  options => options
                      .Seek(at)
                      .WithFrameOutputCount(1)
                      .WithVideoCodec(VideoCodec.Image.Png)
                      .ForceFormat("image2"))
              .ProcessSynchronously();

            stream.Position = 0;


            return stream;
        }

    }
}
