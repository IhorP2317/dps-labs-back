using System.Net;
using System.Text.Json;
using dbs_labs_back.Models;

namespace dbs_labs_back.Middlewares ;

    public class ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, logger);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            ILogger logger)
        {
            // Log the exception
            logger.LogError(exception, "An unhandled exception occurred.");

            // Set the response status code and content type
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            // Create an error response object
            var errorResponse = new ErrorResponse
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Details = exception.InnerException?.Message
            };

            // Serialize the error response to JSON
            var jsonResponse = JsonSerializer.Serialize(errorResponse);

            // Write the response
            await context.Response.WriteAsync(jsonResponse);
        }
    }