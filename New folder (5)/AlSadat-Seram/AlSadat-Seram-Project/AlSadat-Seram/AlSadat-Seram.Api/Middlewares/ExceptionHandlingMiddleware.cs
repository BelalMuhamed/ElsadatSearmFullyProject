using Domain.Common;
using AlSadat_Seram.Api.Responses;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AlSadat_Seram.Api.Middlewares;

/// <summary>
/// Logs every unhandled exception and answers with the standard error envelope,
/// without exposing the exception to the client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    // SQL Server errors that are a conflict with existing data.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;
    private const int ConstraintViolation = 547;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // ApiErrorWriter is Scoped — it must NOT be a constructor parameter here, because
    // middleware itself is constructed once as a singleton. It's injected per-request
    // via the InvokeAsync method parameter instead (standard ASP.NET pattern).
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Runs the rest of the pipeline and turns an unhandled exception into an error response.</summary>
    public async Task InvokeAsync(HttpContext context, ApiErrorWriter errorWriter)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client left: there is nobody to answer and nothing went wrong on our side.
            _logger.LogDebug("Request aborted by the client: {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            // A response that already started streaming cannot be rewritten: let the server abort it.
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            await errorWriter.WriteAsync(context, ToStatusCode(ex), ToCode(ex));
        }
    }

    /// <summary>A specific code when the exception has one; otherwise the status decides (D22).</summary>
    private static string? ToCode(Exception exception) => exception switch
    {
        DbUpdateConcurrencyException => ErrorCodes.ConcurrencyConflict,
        _ => null
    };

    private static HttpStatusCode ToStatusCode(Exception exception) => exception switch
    {
        BadHttpRequestException badRequest => (HttpStatusCode)badRequest.StatusCode,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        DbUpdateConcurrencyException => HttpStatusCode.Conflict,
        DbUpdateException
        {
            InnerException: SqlException
            {
                Number: UniqueIndexViolation or UniqueConstraintViolation or ConstraintViolation
            }
        } => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };
}
