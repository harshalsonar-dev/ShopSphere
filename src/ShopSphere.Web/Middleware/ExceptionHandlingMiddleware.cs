using System.Net;
using System.Text.Json;
using ShopSphere.Application.Common.Exceptions;
using ShopSphere.Application.Common.Models;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var isApiRequest = context.Request.Path.StartsWithSegments("/api");

        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";
        var errors = new List<string>();

        switch (exception)
        {
            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                errors = valEx.Errors.SelectMany(x => x.Value).ToList();
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case BadRequestException badReqEx:
                statusCode = HttpStatusCode.BadRequest;
                message = badReqEx.Message;
                break;

            case DomainException domainEx:
                statusCode = HttpStatusCode.BadRequest;
                message = domainEx.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;

            default:
                _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
                message = "An unexpected server error occurred.";
                break;
        }

        response.StatusCode = (int)statusCode;

        if (isApiRequest)
        {
            var apiResponse = ApiResponse<object>.Fail(message, errors);
            var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await response.WriteAsync(json);
        }
        else
        {
            // For standard browser MVC requests, format a clean response or redirect
            response.ContentType = "text/html";
            await response.WriteAsync($"<div style='font-family:sans-serif;padding:2rem;'><h2 style='color:#dc3545;'>Error ({(int)statusCode})</h2><p>{message}</p><a href='/'>Return to Home</a></div>");
        }
    }
}
