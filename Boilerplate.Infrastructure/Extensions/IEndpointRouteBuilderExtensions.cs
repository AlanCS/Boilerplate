using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Boilerplate.Infrastructure.Extensions
{
    public static class IEndpointRouteBuilderExtensions
    {
        private static string EnvName;

        private static AssemblyName assembly;

        public static void AddCustomHealthCheck(this IEndpointRouteBuilder endpoint, IWebHostEnvironment env)
        {
            EnvName = env.EnvironmentName;
            assembly = Assembly.GetCallingAssembly().GetName();

            endpoint.MapHealthChecks("/healthcheck", new HealthCheckOptions()
            {
                ResponseWriter = WriteResponse
            });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            // copied from https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1
            // then adapted to add app name and version

            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("name", assembly.Name);
                    writer.WriteString("version", assembly.Version.ToString()); // this allows us to check if new versions were really deployed
                    writer.WriteString("environment", EnvName);
                    writer.WriteString("status", result.Status.ToString());
                    writer.WriteString("date", DateTime.Now.ToString("s") + " " + TimeZone.CurrentTimeZone.StandardName);

                    if (result.Entries.Any())
                    {
                        writer.WriteStartObject("results");
                        foreach (var entry in result.Entries)
                        {
                            writer.WriteStartObject(entry.Key);
                            writer.WriteString("status", entry.Value.Status.ToString());
                            writer.WriteString("description", entry.Value.Description);
                            writer.WriteStartObject("data");
                            foreach (var item in entry.Value.Data)
                            {
                                writer.WritePropertyName(item.Key);
                                JsonSerializer.Serialize(
                                    writer, item.Value, item.Value?.GetType() ??
                                    typeof(object));
                            }
                            writer.WriteEndObject();
                            writer.WriteEndObject();
                        }
                        writer.WriteEndObject();
                    }

                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                return context.Response.WriteAsync(json);
            }
        }
    }
}
