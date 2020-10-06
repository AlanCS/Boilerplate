using AutoMapper;
using BizCover.Api.Cars.Infrastructure;
using Boilerplate.Infrastructure;
using Boilerplate.Infrastructure.Exceptions;
using Boilerplate.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Boilerplate.Web
{
    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.Configuration = configuration;
            this.logger = logger;
        }        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IMemoryCacheAdapter, MemoryCacheAdapter>();
            services.AddTransient<IMemoryCache, MemoryCache>();

            services
                .AddMvc()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            services
                .ThrowBadRequestOnBadModelValidation()
                .AddAutoMapper(typeof(Startup))
                .AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly())
                .AddSwaggerGen(c =>
                {
                    c.ExampleFilters();
                })
                .AddHealthChecks();

            // use https://github.com/khellang/Scrutor to register all classes in an assembly (that were not yet registered above)
            // as scoped lifetime
            services.Scan(scan => scan
                    .FromAssemblyOf<Startup>()
                    .AddClasses()
                    .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            services.AddHttpDependencies(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionFilter>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.AddCustomHealthCheck();
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                c.RoutePrefix = string.Empty;
            });

            // useful for many purposes
            // - see cloud consumption and analyse bugs where application restarts unexpectely
            // - test logging is working correctly easily
            // -- check if component tests can intercept logs
            logger.LogInformation("Application is starting");
        }
    }
}
