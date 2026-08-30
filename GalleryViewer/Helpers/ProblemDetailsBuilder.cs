using GalleryViewer.Models;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Helpers
{
    public static class ProblemDetailsBuilder
    {
        public static ObjectResult AsObjectResult(this ProblemDetails problem)
        {
            return new ObjectResult(problem)
            {
                StatusCode = problem.Status
            };
        }

        public static ProblemDetails GenericProblem(string title, string? type, int statusCode, string details)
        {
            return new()
            {
                Type = type,
                Title = title,
                Status = statusCode,
                Detail = details,
            };
        }

        public static ProblemDetails NotFoundProblem(string details)
        {
            return GenericProblem("Entity not found", "/problems/entity-not-found", StatusCodes.Status404NotFound, details);
        }

        public static InvalidTagsProblemDetails InvalidTagsProblem(string title, string? type, int statusCode, string details, IEnumerable<string> tags)
        {
            return new()
            {
                Type = type,
                Title = title,
                Status = statusCode,
                Detail = details,
                Tags = [.. tags]
            };
        }


    }
}
