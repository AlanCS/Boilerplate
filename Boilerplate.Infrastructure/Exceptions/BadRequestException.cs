using System;

namespace Boilerplate.Infrastructure
{
    public class BadRequestException : ApplicationException
    {
        public string InvalidValue { get; set; }

        public BadRequestException(string message, string invalidValue) : base(message)
        {
            this.InvalidValue = invalidValue;
        }
    }
}
