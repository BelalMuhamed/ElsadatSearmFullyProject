using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
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
        public NotFoundException(string message) : base(message) { }

    }
}
