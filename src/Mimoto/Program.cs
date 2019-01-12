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

            var serviceProvider = host.Services;
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    applicationDbContext.Database.Migrate();

                    var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                    configurationDbContext.Database.Migrate();

                    var persistedGrantDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
                    persistedGrantDbContext.Database.Migrate();

                    if (!configurationDbContext.Clients.Any())
                    {
                        foreach (var client in Config.GetClients())
                        {
                            configurationDbContext.Clients.Add(client.ToEntity());
                        }
                        configurationDbContext.SaveChanges();
                    }

                    if (!configurationDbContext.IdentityResources.Any())
                    {
                        foreach (var resource in Config.GetIdentityResources())
                        {
                            configurationDbContext.IdentityResources.Add(resource.ToEntity());
                        }
                        configurationDbContext.SaveChanges();
                    }

                    if (!configurationDbContext.ApiResources.Any())
                    {
                        foreach (var resource in Config.GetApis())
                        {
                            configurationDbContext.ApiResources.Add(resource.ToEntity());
                        }
                        configurationDbContext.SaveChanges();
                    }
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
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