using System;
using System.Reflection;
using IdentityServer4;
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
            var connectionString = _config.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Mimoto.Identity";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            services.AddMvc();

            var identityBuilder = services.AddIdentityServer()
                .AddAspNetIdentity<ApplicationUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                });

            if (_env.IsDevelopment())
            {
                // identityBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = _config.GetValue<string>("google:clientid");
                    options.ClientSecret = _config.GetValue<string>("google:clientsecret");
                })
                .AddFacebook("Facebook", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = _config.GetValue<string>("facebook:clientid");
                    options.ClientSecret = _config.GetValue<string>("facebook:clientsecret");
                })
                .AddMicrosoftAccount("Microsoft", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = _config.GetValue<string>("microsoft:clientid");
                    options.ClientSecret = _config.GetValue<string>("microsoft:clientsecret");
                })
                .AddGitHub("GitHub", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = _config.GetValue<string>("microsoft:clientid");
                    options.ClientSecret = _config.GetValue<string>("microsoft:clientsecret");
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}