using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Models
{
    public sealed class InvalidTagsProblemDetails : ProblemDetails
    {
        public required IReadOnlyList<string> Tags { get; init; }
    }
}
