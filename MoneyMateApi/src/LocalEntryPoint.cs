using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;

namespace MoneyMateApi
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.MinimumLevel.Information().WriteTo.Console(new JsonFormatter());
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var environment = context.HostingEnvironment.EnvironmentName;
                    if (environment != "dev")
                    {
                        builder.AddSystemsManager("/");
                        builder.AddSystemsManager($"/{environment}");
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}