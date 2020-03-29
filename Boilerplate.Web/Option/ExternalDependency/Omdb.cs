using Boilerplate.Infrastructure.Configuration;

namespace Boilerplate.Web.Option.ExternalDependency
{
    public class Omdb : HttpClientSettings
    {
        public string ApiKey { get; set; }
    }
}
