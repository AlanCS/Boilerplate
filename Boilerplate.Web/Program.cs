using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Boilerplate.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Build(args).Run();
        }

        public static IWebHost Build(string[] args) => CreateHostBuilder(args).Build();

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>();
    }
}
