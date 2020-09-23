using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Boilerplate.Infrastructure.Exceptions
{
    public class ExceptionFilter
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionFilter> logger;

        public ExceptionFilter(RequestDelegate next, ILogger<ExceptionFilter> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next delegate/middleware in the pipeline
                await next(context);
            }
            catch (Exception ex)
            {
                var result = HandleError(ex);
                context.Response.StatusCode = (int)result.code;

                await context.Response.WriteAsync(JsonSerializer.Serialize(result));
            }
        }

        internal (ErrorResponse response, HttpStatusCode code) HandleError(Exception exception)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = Messages.DefaultErrorMessage; // we hide the real reason for the exception, to not help hackers; but could be different approach if internal api

            switch (exception)
            {
                case BaseException ex:
                    code = ex.HttpStatus;
                    logger.Log(ex.logLevel, ex.Message, ex);
                    break;
                default:
                    logger.LogCritical(exception.Message, exception); // catch all, shouldn't happen theoretically
                    break;
            }

            return (new ErrorResponse() { Message = message }, code);
        }
    }
}
