using AppointmentSaaS.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace AppointmentSaaS.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation Error", (object)ve.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, "Not Found", (object)new { message = nfe.Message }),
            ForbiddenException fe => (HttpStatusCode.Forbidden, "Forbidden", (object)new { message = fe.Message }),
            Domain.Exceptions.DomainException de => (HttpStatusCode.UnprocessableEntity, "Domain Error", (object)new { message = de.Message }),
            _ => (HttpStatusCode.InternalServerError, "Server Error", (object)new { message = "An unexpected error occurred." })
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { title, status = (int)statusCode, errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
