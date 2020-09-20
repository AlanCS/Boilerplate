using Boilerplate.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using SystemTestingTools;
using Xunit;

namespace Boilerplate.ComponentTests.Setup
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }

        public string StubsFolder { get; private set; }

        public TestServerFixture()
        {
            StartServer();
            SanityCheckServer();
        }

        private void StartServer()
        {
            StubsFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\Stubs";

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // use the same settings as the website (linked referenced)
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    // make small changes to configuration, such as disabling caching
                    config.AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true);
                })
                .InterceptHttpCallsBeforeSending()
                .IntercepLogs(minimumLevelToIntercept: LogLevel.Information,
                                namespaceToIncludeStart: new[] { "Boilerplate" })
                .UseEnvironment("Development");

            Server = new TestServer(builder);
        }

        /// <summary>
        /// Run a quick sanity check, before running any tests
        /// </summary>
        /// <returns></returns>
        private void SanityCheckServer()
        {
            HttpResponseMessage response = null;
            using (var client = Server.CreateClient())
                response = client.GetAsync("/healthcheck").Result; // we run the async method synchronously because it's called from a contructor, that can't be async

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApplicationException("TestServer doesn't respond to basic request to /healthcheck");

            if (UnsessionedData.UnsessionedLogs.Count > 1)
                throw new ApplicationException("Unexpected logs are showing up when starting application");

            if (UnsessionedData.UnsessionedLogs.FirstOrDefault()?.ToString() != "Information: Application is starting")
                throw new ApplicationException("Logs don't seem to be wired correctly");
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }

    [CollectionDefinition("SharedServer collection")]
    public class SharedServerCollection : ICollectionFixture<TestServerFixture>
    {
        // as suggested in https://xunit.github.io/docs/shared-context
        // done so this collection is shared between many tests
    }
}
