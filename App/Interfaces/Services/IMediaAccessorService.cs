using System.Diagnostics.CodeAnalysis;

namespace App.Interfaces.Services
{
    public interface IMediaAccessorService
    {
        bool Exists([NotNullWhen(true)] string? assetDataRef);
        DateTime GetCreationTime(string assetDataRef);
        DateTime GetLastModifiedTime(string assetDataRef);
        Stream GetMediaData(string assetDataRef);
    }
}
