using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace AlSadat_Seram.Api.Responses;

/// <summary>
/// Builds and writes every error that does not come from a service (model validation,
/// 401/403/404/405/415/429 and unhandled exceptions) in the same envelope as
/// <see cref="Result{T}"/>, with a localized message, a stable code and a trace id.
/// </summary>
public sealed class ApiErrorWriter(ILocalizationService localization, IOptions<JsonOptions> jsonOptions)
{
    private const string BodyField = "body";

    /// <summary>Builds a localized failure; without a code it uses the general code of the status.</summary>
    public Result<object> Create(HttpContext context, HttpStatusCode statusCode, string? code = null)
    {
        var result = Result<object>.FailureKey(code ?? ErrorCodes.FromStatus(statusCode), statusCode);
        return Complete(context, result);
    }

    /// <summary>Builds the 400 result of an invalid model: field errors, or one error for an unreadable body.</summary>
    public Result<object> CreateValidation(HttpContext context, ModelStateDictionary modelState)
    {
        var invalid = modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToList();

        // An unreadable body also reports its parameter as "required": keep only the real cause,
        // and never the parser's own text.
        var bodyKeys = invalid.Select(entry => entry.Key).Where(IsBodyKey).ToList();
        var errors = bodyKeys.Count > 0
            ? bodyKeys
                .Select(ToFieldName)
                .Distinct()
                .ToDictionary(field => field, _ => new[] { localization.Resolve(ErrorCodes.InvalidBody) })
            : invalid
                .GroupBy(entry => ToFieldName(entry.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(entry => entry.Value!.Errors)
                        .Select(error => localization.Resolve(error.ErrorMessage))
                        .ToArray());

        var code = bodyKeys.Count > 0 ? ErrorCodes.InvalidBody : ErrorCodes.ValidationFailed;
        return Complete(context, Result<object>.ValidationFailure(errors, code));
    }

    /// <summary>Writes a localized failure as the response.</summary>
    public Task WriteAsync(HttpContext context, HttpStatusCode statusCode, string? code = null)
    {
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsJsonAsync(
            Create(context, statusCode, code),
            jsonOptions.Value.JsonSerializerOptions,
            context.RequestAborted);
    }

    private Result<object> Complete(HttpContext context, Result<object> result)
    {
        result.Message = localization.Resolve(result.MessageKey!);
        result.TraceId = Activity.Current?.Id ?? context.TraceIdentifier;
        return result;
    }

    private static bool IsBodyKey(string key) => key.Length == 0 || key[0] == '$';

    // "$.userName" / "Address.City" -> "userName" / "address.city"; an empty path is the body itself.
    private static string ToFieldName(string key)
    {
        var path = key.TrimStart('$').TrimStart('.');
        return path.Length == 0
            ? BodyField
            : string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }
}
