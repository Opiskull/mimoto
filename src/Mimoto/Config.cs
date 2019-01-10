using IdentityServer4.Models;
using System.Collections.Generic;

namespace Mimoto
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[] { };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new Client[] { };
        }
    }
}