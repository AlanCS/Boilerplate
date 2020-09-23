using Boilerplate.Web.Proxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace Boilerplate.Web
{
    public static class StartupExtensions
    {
        public static void AddHttpDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Option.ExternalDependency.Omdb>(configuration.GetSection("externalDependencies:Omdb"));
            var omdb = services.BuildServiceProvider().GetService<IOptions<Option.ExternalDependency.Omdb>>().Value;
            if(string.IsNullOrWhiteSpace(omdb?.BaseAddress)) throw new Exception("Omdb is not configured correctly in settings file");

            Constants.OmdbApiKey = omdb?.ApiKey ?? throw new Exception("Omdb is not configured correctly in settings file");

            // setup copied from https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
                .RetryAsync(omdb.Retries);

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(omdb.TimeoutIndividualTrySeconds);  // Timeout for an individual try

            services
                .AddHttpClient<IMovieDatabaseProxy, MovieDatabaseProxy>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy) // We place the timeoutPolicy inside the retryPolicy, to make it time out each try
                .ConfigureHttpClient(c => {
                    c.BaseAddress = new Uri(omdb.BaseAddress);
                    c.DefaultRequestHeaders.Add("Referer", Constants.Referer);
                    c.Timeout = TimeSpan.FromSeconds(omdb.TimeoutGlobalSeconds); // Overall timeout across all tries
                });
        }
    }
}
