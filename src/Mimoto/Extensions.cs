
using System;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mimoto
{
    public static class Extensions
    {
        public static ExternalAuthenticationBuilder AddExternalAuthentication(this IServiceCollection collection, IConfiguration config)
        {
            return new ExternalAuthenticationBuilder(collection.AddAuthentication(), config);
        }
    }
}