using System.Collections.Generic;
using System.Net;

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
        /// </summary>
        public string? MessageKey { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// Field-level validation errors (ModelState / DataAnnotations shape).
        /// Null for non-validation results.
        /// </summary>
        public IDictionary<string, string[]>? Errors { get; set; }

        public static Result<T> Success(T data, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new() { IsSuccess = true, Data = data, Message = message, StatusCode = statusCode };

        public static Result<T> Success(T data, HttpStatusCode statusCode)
            => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

        public static Result<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new() { IsSuccess = false, Message = message, StatusCode = statusCode };

        /// <summary>
        /// Localized failure — pass a key from LocalizationResources instead of a literal string.
        /// Use this in new/migrated code; old Failure(string) calls are unaffected.
        /// </summary>
        public static Result<T> FailureKey(string messageKey, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new() { IsSuccess = false, MessageKey = messageKey, StatusCode = statusCode };

        public static Result<T> ValidationFailure(IDictionary<string, string[]> errors, string? messageKey = "Common.ValidationFailed")
            => new()
            {
                IsSuccess = false,
                MessageKey = messageKey,
                Errors = errors,
                StatusCode = HttpStatusCode.BadRequest
            };
    }
}