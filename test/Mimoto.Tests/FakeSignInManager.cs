using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimoto.Models;
using Moq;

namespace Mimoto.Tests
{
    public class FakeSignInManager : SignInManager<ApplicationUser>
    {
        public FakeSignInManager()
                : base(new Mock<FakeUserManager>().Object,
                    new Mock<IHttpContextAccessor>().Object,
                    new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                    new Mock<IAuthenticationSchemeProvider>().Object)
            { }        

        public override Task SignOutAsync(){
            return Task.CompletedTask;
        }

        public override Task<ClaimsPrincipal> CreateUserPrincipalAsync(ApplicationUser user){
            return Task.FromResult(                        
                new System.Security.Claims.ClaimsPrincipal(
                    new ClaimsIdentity(new [] {
                        new Claim(JwtClaimTypes.Subject, "test1"),
                        new Claim(JwtClaimTypes.Name, "test1")
                        }
                    )
                ));
        }
    }
}