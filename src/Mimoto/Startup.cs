using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mimoto.Database;
using Mimoto.Models;
using Mimoto.Quickstart;

namespace Mimoto
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
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
                SecurityKey key = new RsaSecurityKey(RSACryptoServiceProvider.Create(512));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
                identityBuilder.AddSigningCredential(signingCredentials);
            }

            services.AddExternalAuthentication(_config)
                .AddIfExists("google", (p, a, c) => a.AddGoogle(p, c))
                .AddIfExists("facebook", (p, a, c) => a.AddFacebook(p, c))
                .AddIfExists("microsoft", (p, a, c) => a.AddMicrosoftAccount(p, c))
                .AddIfExists("github", (p, a, c) => a.AddGitHub(p, c));
            
            services.AddSingleton<IWindowsPrincipalProvider, WindowsPrincipalProvider>();
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
                options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            };
        }
    }
}