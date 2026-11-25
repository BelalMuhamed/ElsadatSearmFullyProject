using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace Domain.Common
{
    /// <summary>
    /// Standard API response envelope. Every controller action should return this
    /// (wrapped by BaseApiController.HandleResult) so the frontend gets one predictable shape.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Final, human-readable message. If <see cref="MessageKey"/> is set, this gets
        /// overwritten with the localized text at the API boundary (see BaseApiController).
        /// If MessageKey is null, this is used as-is — this is what keeps every existing
        /// hardcoded Arabic message in the codebase working unchanged.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Optional lookup key (e.g. "Supplier.DuplicatePhone") resolved against
        /// LocalizationResources based on the request's Accept-Language header.
        /// Leave null to keep using a literal Message — nothing breaks if you never set this.
        /// Not serialized: a failure exposes it as <see cref="Code"/>.
        /// </summary>
        [JsonIgnore]
        public string? MessageKey { get; set; }

        /// <summary>
        /// Stable error code for the client (D22): the message key, or the general code of the
        /// status when the failure only has a literal message. Null on success.
        /// </summary>
        public string? Code => IsSuccess ? null : MessageKey ?? ErrorCodes.FromStatus(StatusCode);

        public HttpStatusCode StatusCode { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// Field-level validation errors (ModelState / DataAnnotations shape).
        /// Null for non-validation results.
        /// </summary>
        public IDictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Request trace id that finds the failure in the logs. Set at the API boundary, null on success.
        /// </summary>
        public string? TraceId { get; set; }

        public static Result<T> Success(T data, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new() { IsSuccess = true, Data = data, Message = message, StatusCode = statusCode };

        public static Result<T> Success(T data, HttpStatusCode statusCode)
            => new() { IsSuccess = true, Data = data, StatusCode = statusCode };
        /// <summary>
        /// Localized success — pass a key from LocalizationResources instead of a literal string.
        /// Mirrors FailureKey; resolved at the API boundary by BaseApiController.HandleResult.
        /// </summary>
        public static Result<T> SuccessKey(string messageKey, T data, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new() { IsSuccess = true, Data = data, MessageKey = messageKey, StatusCode = statusCode };
        public static Result<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new() { IsSuccess = false, Message = message, StatusCode = statusCode };

        /// <summary>
        /// Localized failure — pass a key from LocalizationResources instead of a literal string.
        /// Use this in new/migrated code; old Failure(string) calls are unaffected.
        /// </summary>
        public static Result<T> FailureKey(string messageKey, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new() { IsSuccess = false, MessageKey = messageKey, StatusCode = statusCode };

        public static Result<T> ValidationFailure(IDictionary<string, string[]> errors, string? messageKey = ErrorCodes.ValidationFailed)
            => new()
            {
                IsSuccess = false,
                MessageKey = messageKey,
                Errors = errors,
                StatusCode = HttpStatusCode.BadRequest
            };
    }
}