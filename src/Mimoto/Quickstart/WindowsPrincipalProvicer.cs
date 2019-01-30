using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;

namespace Mimoto.Quickstart
{
    // We are excluding this file from code coverage as we cannot test it with linux
    [ExcludeFromCodeCoverage]
    public class WindowsPrincipalProvider : IWindowsPrincipalProvider
    {
        public bool IsWindowsPrincipal(ClaimsPrincipal principal)
        {
            return principal is WindowsPrincipal; 
        }

        public IEnumerable<Claim> Groups(IIdentity identity){
            var wi = identity as WindowsIdentity;
            var groups = wi.Groups.Translate(typeof(NTAccount));
            return groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
        }
    }
}