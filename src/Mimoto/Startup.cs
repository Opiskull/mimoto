using System;
using System.Reflection;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimoto.Database;
using Mimoto.Models;

namespace Mimoto
{
    public class Startup
    {
        public readonly IHostingEnvironment _env;
        public readonly IConfiguration _config;
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            _env = environment;
            _config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services
                .AddDbContext<ApplicationDbContext>(ConfigureDb())
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Mimoto.Identity";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            var identityBuilder = services.AddIdentityServer()
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = ConfigureDb();
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = ConfigureDb();

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                });

            if (_env.IsDevelopment())
            {
                identityBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddExternalAuthentication(_config)
                .AddIfExists("google", (p, a, c) => a.AddGoogle(p, c))
                .AddIfExists("facebook", (p, a, c) => a.AddFacebook(p, c))
                .AddIfExists("microsoft", (p, a, c) => a.AddMicrosoftAccount(p, c))
                .AddIfExists("github", (p, a, c) => a.AddGitHub(p, c));
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles()
                .UseDefaultFiles()
                .UseIdentityServer()
                .UseMvcWithDefaultRoute();
        }

        private Action<DbContextOptionsBuilder> ConfigureDb()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            return (options) =>
            {
                options.UseSqlite(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            };
        }
    }
}