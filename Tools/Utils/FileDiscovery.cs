using Tools.Models;

namespace Tools.Utils
{
    internal static class FileDiscovery
    {
        private static readonly string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avi", ".mp4", ".webm", ".gif" };

        public static IReadOnlyList<string> AllowedExtensions => allowedExtensions;

        public static (IEnumerable<FilesGroup>, int) Discover(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            string root = @$"{path}";
            if (!Directory.Exists(root))
            {
                return ([], 0);
            }

            SearchOption searchOption = SearchOption.AllDirectories;

            IEnumerable<string> files = Directory
                .EnumerateFiles(root, "*.*", searchOption)
                .Where(f => allowedExtensions.Contains(
                    Path.GetExtension(f).ToLowerInvariant()));

            int filesCount = files.Count();

            IEnumerable<FilesGroup> groupedFiles = files
                .OrderBy(File.GetLastWriteTimeUtc)
                .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
                .GroupBy(f =>
                {
                    var relativePath = Path.GetRelativePath(root, f);
                    var folder = Path.GetDirectoryName(relativePath);

                    return folder ?? "";
                })
                .Select(x => new FilesGroup(
                    x.Key,
                    x.Select(f => Path.GetRelativePath(root, f))
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                        .ToArray())
                );


            return (groupedFiles, filesCount);
        }
    }
}
