using System;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mimoto.Database
{
    public class Migrations
    {
        public static void Migrate(IServiceProvider serviceProvider){            
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var appDb = scope.ServiceProvider.GetService<ApplicationDbContext>();
                appDb.Database.Migrate();

                var configDb = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                configDb.Database.Migrate();

                var grantDb = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
                grantDb.Database.Migrate();
            }
        }

        public static void AddConfigToDb(IServiceProvider serviceProvider){
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var configurationDb = scope.ServiceProvider.GetService<ConfigurationDbContext>();

                if (!configurationDb.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        configurationDb.Clients.Add(client.ToEntity());
                    }
                    configurationDb.SaveChanges();
                }

                if (!configurationDb.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        configurationDb.IdentityResources.Add(resource.ToEntity());
                    }
                    configurationDb.SaveChanges();
                }

                if (!configurationDb.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApis())
                    {
                        configurationDb.ApiResources.Add(resource.ToEntity());
                    }
                    configurationDb.SaveChanges();
                }
            }
        }
    }
}