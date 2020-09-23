using Boilerplate.Infrastructure.Exceptions;
using System;

namespace Boilerplate.Infrastructure
{
    // useful to have exceptions specializing in downstream dependencies, so we can easily tell them apart in logs
    public class DownstreamException : BaseException
    {
        public DownstreamException(string message, Exception innerException = null) : base(message, innerException)
        {

        }
    }
}
