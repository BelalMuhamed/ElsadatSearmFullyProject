using System.Net;

namespace Domain.Common;

/// <summary>
/// Stable error codes shared by every endpoint (D22). A code is also the localization key of its message.
/// </summary>
public static class ErrorCodes
{
    public const string BadRequest = "Common.BadRequest";
    public const string InvalidBody = "Common.InvalidBody";
    public const string ValidationFailed = "Common.ValidationFailed";
    public const string Unauthorized = "Common.Unauthorized";
    public const string PermissionDenied = "Common.PermissionDenied";
    public const string NotFound = "Common.NotFound";
    public const string MethodNotAllowed = "Common.MethodNotAllowed";
    public const string DataConflict = "Common.DataConflict";
    public const string UnsupportedMediaType = "Common.UnsupportedMediaType";
    public const string TooManyRequests = "Common.TooManyRequests";
    public const string ServerError = "Common.ServerError";

    /// <summary>The general code of an HTTP status, used when a failure has no code of its own.</summary>
    public static string FromStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => Unauthorized,
        HttpStatusCode.Forbidden => PermissionDenied,
        HttpStatusCode.NotFound => NotFound,
        HttpStatusCode.MethodNotAllowed => MethodNotAllowed,
        HttpStatusCode.Conflict => DataConflict,
        HttpStatusCode.UnsupportedMediaType => UnsupportedMediaType,
        HttpStatusCode.TooManyRequests => TooManyRequests,
        _ when (int)statusCode >= 500 => ServerError,
        _ => BadRequest
    };
}
