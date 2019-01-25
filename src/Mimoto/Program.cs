using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimoto.Database;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;

namespace Mimoto
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            Migrations.Migrate(host.Services);
            Migrations.AddConfigToDb(host.Services);
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .AddYamlFile("appsettings.yml", optional: false, reloadOnChange: false)
                        .AddYamlFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.yml", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.File(@"mimoto.log")
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                });
        }
    }
}