using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T? Data { get; set; }

        public static Result<T> Success(T data,string? message = null,HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static Result<T> Success(
                T data,
                HttpStatusCode statusCode)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = statusCode
            };
        }
        public static Result<T> Failure(string message,HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}