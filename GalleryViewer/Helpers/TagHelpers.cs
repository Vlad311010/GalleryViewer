using Shared;

namespace GalleryViewer.Helpers
{
    public static class TagHelpers
    {
        public static string NormalizeTag(this string tag)
        {
            return tag
                .TrimStart()
                .TrimEnd()
                .ToLower()
                .Replace(Constants.TAG_SPACE_CHARACTER, Constants.SPACE_CHARACTER);
        }
    }
}
