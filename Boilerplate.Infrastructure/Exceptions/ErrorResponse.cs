namespace Boilerplate.Infrastructure.Exceptions
{
    public class ErrorResponse
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
