using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Mimoto.Quickstart{
    public interface IWindowsPrincipalProvider
    {
        bool IsWindowsPrincipal(ClaimsPrincipal principal);

        IEnumerable<Claim> Groups(IIdentity identity);
    }
}