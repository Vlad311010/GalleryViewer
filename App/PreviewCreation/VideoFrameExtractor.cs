
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace App.PreviewCreation
{
    internal static class VideoFrameExtractor
    {
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
