using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mimoto.Services
{
    public class AuthLoader
    {
        private IConfiguration config;
        public AuthLoader(IConfiguration config){
            this.config = config;
        }

        public void AddIfExists(String name, Action<IConfigurationSection> sectionAction){
            var section = config.GetSection(name);
            if(section.Exists()){
                sectionAction(section);
            }
        }
    }
}