using App.Exceptions;

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

        private static async Task HandleAppExceptionAsync(HttpContext context, AppException exception)
        {
            var (statusCode, title) = exception switch
            {
                EntityNotFoundException =>
                    (StatusCodes.Status404NotFound, "Entity not found"),

                _ =>
                    (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            var response = new
            {
                title,
                status = statusCode,
                detail = statusCode == 500
                    ? "An unexpected error occurred."
                    : exception.Message
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
