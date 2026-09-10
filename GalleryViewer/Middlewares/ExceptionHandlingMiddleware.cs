using App.Exceptions;
using FluentValidation;
using GalleryViewer.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            switch (exception)
            {
                case ValidationException validationException:
                    await HandleValidationExceptionAsync(context, validationException);
                    break;

                case AppException appException:
                    await HandleAppExceptionAsync(context, appException);
                    break;

                default:
                    context.Response.StatusCode =
                        StatusCodes.Status500InternalServerError;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = 500,
                        title = "Internal server error",
                        detail = "An unexpected error occurred."
                    });
                    break;
            }
        }

        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Type = "/problems/validation"
            };

            context.Response.StatusCode = problem.Status.Value;

            await context.Response.WriteAsJsonAsync(problem);
        }

        private static async Task HandleAppExceptionAsync(HttpContext context, AppException exception)
        {
            var (statusCode, type, title) = exception switch
            {
                EntityNotFoundException =>
                    (StatusCodes.Status404NotFound, "/problems/entity-not-found", "Not found"),

                EntityAlreadyExistsException =>
                    (StatusCodes.Status409Conflict, "/problems/entity-already-exists", "Alredy exists"),

                MediaNotFoundException =>
                    (StatusCodes.Status404NotFound, "/problems/media-not-found", "Media not found"),

                UnknownTagsException =>
                    (StatusCodes.Status400BadRequest, "/problems/unknown-tag", "One or more tags are not defined"),

                _ =>
                    (StatusCodes.Status500InternalServerError, null, "Internal server error")
            };

            ProblemDetails problem;
            switch (exception)
            {
                case UnknownTagsException e:
                    problem = ProblemDetailsBuilder.InvalidTagsProblem(title, type, statusCode, title, e.InvalidTags);
                    break;

                default:
                    problem = ProblemDetailsBuilder.GenericProblem(title, type, statusCode, statusCode == 500 ? "An unexpected error occurred." : title);
                    break;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
