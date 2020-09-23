using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Boilerplate.Infrastructure.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException(string message, Exception innerException = null) : base(message, innerException)
        {

        }
        public HttpStatusCode HttpStatus = HttpStatusCode.InternalServerError;
        public LogLevel logLevel = LogLevel.Error;
    }
}
