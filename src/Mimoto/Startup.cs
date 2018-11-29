using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimoto.Services;

namespace Mimoto
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration config){
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer();

            var auth = services.AddAuthentication();
            var authLoader = new AuthLoader(_config);
            authLoader.AddIfExists("google", section => auth.AddGoogle(options => section.Bind(options)));
            authLoader.AddIfExists("microsoft", section => auth.AddMicrosoftAccount(options => section.Bind(options)));
            authLoader.AddIfExists("facebook", setion => auth.AddFacebook(options => setion.Bind(options)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
        }
    }
}
