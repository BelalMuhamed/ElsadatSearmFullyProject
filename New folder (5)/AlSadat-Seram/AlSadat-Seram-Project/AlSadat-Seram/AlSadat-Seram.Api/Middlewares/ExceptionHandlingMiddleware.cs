using Domain.Common;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace AlSadat_Seram.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled Exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = ex switch
        {
            BusinessException be => (int)be.StatusCode,
            NotFoundException => 404,
            UnauthorizedAccessException => 401,
            ArgumentException => 400,
            DbUpdateException => 409,
            _ => 500
        };

        string message = ex switch
        {
            BusinessException be => be.Message,
            UnauthorizedAccessException uae when uae.Message.Contains("Google token") => "Invalid Google token",
            NotFoundException => "Resource not found",
            UnauthorizedAccessException => "Unauthorized access",
            ArgumentException => "Bad request",
            DbUpdateException => "Database conflict occurred",
            _ => "An internal server error has occurred. Please try again later."
        };

        response.StatusCode = statusCode;

        var result = Result<string>.Failure(
            message,
            (HttpStatusCode)statusCode
        );

        var jsonResponse = JsonSerializer.Serialize(result);

        await response.WriteAsync(jsonResponse);
    }
}
