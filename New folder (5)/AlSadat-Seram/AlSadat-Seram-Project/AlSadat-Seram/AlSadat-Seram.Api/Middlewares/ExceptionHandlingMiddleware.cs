using Application.Services.contract.LocalizationService;
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

    // ILocalizationService is Scoped — it must NOT be a constructor parameter here,
    // because middleware itself is constructed once as a singleton. It's injected
    // per-request via the InvokeAsync method parameter instead (standard ASP.NET pattern).
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILocalizationService localization)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex, localization);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILocalizationService localization)
    {
        // Safety: if a response has already started streaming, we can't rewrite it.
        if (context.Response.HasStarted)
            return;

        var (statusCode, messageKeyOrLiteral) = Map(ex);
        var message = localization.Resolve(messageKeyOrLiteral);

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var result = Result<object>.Failure(message, (HttpStatusCode)statusCode);

        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }

    private static (int StatusCode, string MessageKeyOrLiteral) Map(Exception ex) => ex switch
    {
        BusinessException be => ((int)be.StatusCode, be.MessageKey),
        NotFoundException nf => (404, nf.MessageKey),
        UnauthorizedAccessException uae when uae.Message.Contains("Google token") => (401, "Auth.InvalidGoogleToken"),
        UnauthorizedAccessException => (401, "Common.Unauthorized"),
        ArgumentException => (400, "Common.BadRequest"),
        DbUpdateConcurrencyException => (409, "Common.DataConflict"),
        DbUpdateException => (409, "Common.DataConflict"),
        _ => (500, "Common.ServerError")
    };
}