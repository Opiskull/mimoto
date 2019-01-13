
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
        // public static AuthenticationProviderBuilder AddProviderIfExists(this AuthenticationBuilder builder, IConfiguration configuration,
        //     string provider, Action<string, AuthenticationBuilder, Action<OAuthOptions>> action)
        // {
        //     var section = configuration.GetSection($"providers:{provider}");
        //     if (section.Exists())
        //     {
        //         action(provider, builder, options =>
        //         {
        //             section.Bind(options);
        //             options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        //         });
        //     }
        //     return builder;
        // }

        public static ExternalAuthenticationBuilder AddAuthenticationProviders(this IServiceCollection collection, IConfiguration config)
        {
            return new ExternalAuthenticationBuilder(collection.AddAuthentication(), config);
        }
    }
}