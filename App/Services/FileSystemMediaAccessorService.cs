using App.Interfaces.Services;
using System.Diagnostics.CodeAnalysis;

namespace App.Services
{
    public class FileSystemMediaAccessorService : IMediaAccessorService
    {
        public bool Exists([NotNullWhen(true)] string? assetDataRef)
        {
            return !string.IsNullOrWhiteSpace(assetDataRef) && File.Exists(assetDataRef);
        }

        public Stream GetMediaData(string assetDataRef)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assetDataRef);

            return new FileStream(assetDataRef, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public DateTime GetCreationTime(string assetDataRef)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assetDataRef);
            return File.GetCreationTimeUtc(assetDataRef);
        }

        public DateTime GetLastModifiedTime(string assetDataRef)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assetDataRef);
            return File.GetLastWriteTimeUtc(assetDataRef);
        }
    }


}
