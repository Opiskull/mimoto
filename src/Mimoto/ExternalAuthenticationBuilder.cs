using System;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;

namespace Mimoto
{
    public class ExternalAuthenticationBuilder
    {
        private readonly AuthenticationBuilder _authBuilder;
        private readonly IConfiguration _config;
        public ExternalAuthenticationBuilder(AuthenticationBuilder authBuilder, IConfiguration config)
        {
            _authBuilder = authBuilder;
            _config = config;
        }
        public ExternalAuthenticationBuilder AddIfExists(string provider, Action<string, AuthenticationBuilder, Action<OAuthOptions>> action)
        {
            var section = _config.GetSection($"providers:{provider}");
            if (section.Exists())
            {
                action(provider, _authBuilder, options =>
                {
                    section.Bind(options);
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                });
            }
            return this;
        }
    }
}