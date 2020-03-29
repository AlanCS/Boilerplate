namespace Boilerplate.Infrastructure.Configuration
{
    public class HttpClientSettings
    {
        public string BaseAddress { get; set; }
        public int TimeoutIndividualTrySeconds { get; set; }
        public int TimeoutGlobalSeconds { get; set; }
        public int Retries { get; set; }
        public int CircuitBreakerPercentage { get; set; }
        public int CircuitBreakerOpenTimeSeconds { get; set; }
    }
}
