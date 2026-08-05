using Microsoft.AspNetCore.StaticFiles;

namespace App.Extensions
{
    public static class FileExtensions
    {
        private static readonly FileExtensionContentTypeProvider Provider = new();

        public static string ToMimeType(this string filePath)
        {
            return Provider.TryGetContentType(filePath, out var mimeType)
                ? mimeType
                : "application/octet-stream";
        }
    }
}
