using System;
using System.Net;

namespace Domain.Exceptions
{
    /// <summary>
    /// Thrown for expected business-rule violations.
    /// <para>
    /// Pass either a literal message ("يوجد مندوب بنفس الايميل" — works exactly as before,
    /// zero changes needed to existing throw sites) or a lookup key ("Representative.DuplicateEmail" —
    /// resolved via LocalizationResources by the exception middleware). The middleware tries the
    /// dictionary first; if the key isn't found, it falls back to treating the string as the literal
    /// message. This is what makes localization adoptable module-by-module.
    /// </para>
    /// </summary>
    public class BusinessException : Exception
    {
        public string MessageKey { get; }
        public HttpStatusCode StatusCode { get; }

        public BusinessException(string messageKey, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(messageKey)
        {
            MessageKey = messageKey;
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : Exception
    {
        public string MessageKey { get; }

        public NotFoundException(string messageKey) : base(messageKey)
        {
            MessageKey = messageKey;
        }
    }
}