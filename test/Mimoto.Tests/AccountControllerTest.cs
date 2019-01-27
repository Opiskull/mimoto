using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mimoto.Models;
using Mimoto.Quickstart.Account;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class AccountControllerTest{
        private readonly FakeUserManager _userManager;
        private readonly FakeSignInManager _signInManager;
        private readonly Mock<IIdentityServerInteractionService> _interaction;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IAuthenticationSchemeProvider> _schemeProvider;
        private readonly Mock<IEventService> _events;

        public AccountControllerTest(){
            _userManager = new FakeUserManager();
            _signInManager = new FakeSignInManager();
            _interaction = new Mock<IIdentityServerInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _schemeProvider = new Mock<IAuthenticationSchemeProvider>();
            _events = new Mock<IEventService>();
        }

        [Fact]
        public async Task LoginRedirectToExternal()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ 
                    ClientId = "client1",
                    ScopesRequested = new [] { "api1"},
                    IdP = "idp"
                });
            var controller = CreateController();

            var redirectToAction = await controller.Login("~/");

            redirectToAction.Should().BeAssignableTo<RedirectToActionResult>();
            redirectToAction.As<RedirectToActionResult>().ControllerName.Should().Be("External");
            redirectToAction.As<RedirectToActionResult>().ActionName.Should().Be("Challenge");
        }

        [Fact]
        public async Task LoginView()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ 
                    ClientId = "client1",
                    ScopesRequested = new [] { "api1"}
                });
            _schemeProvider.Setup(s => s.GetAllSchemesAsync()).ReturnsAsync(new [] {
                new AuthenticationScheme("test", "Test", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("test1", "Test 1", typeof(IAuthenticationHandler))
            });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client{
                    Enabled = true,
                    IdentityProviderRestrictions = new [] {"test","test1"}
                });

            var controller = CreateController();
            var viewResult = await controller.Login("~/");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().Model.As<LoginViewModel>().ExternalProviders.Should().NotBeEmpty().And.HaveCount(2);
        }

        [Fact]
        public async Task Logout(){
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { 
                    User = new ClaimsPrincipal(new ClaimsIdentity(new []
                        {
                            new Claim("sub","user1"),
                            new Claim(JwtClaimTypes.IdentityProvider, "testIdp")
                        },"someAuth")
                    )
                }
            };

            var viewResult = await controller.Logout("blub");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().Model.As<LoggedOutViewModel>();
        }

        [Fact]
        public async Task LogoutWithoutUser(){
            var user = new Mock<IIdentity>();
            user.SetupGet(x => x.IsAuthenticated).Returns(false);
            user.SetupGet(x => x.Name).Returns("user1");

            var controller = CreateController();
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { 
                    User = new ClaimsPrincipal(user.Object)                   
                }
            };

            var viewResult = await controller.Logout("blub");
        }

        private AccountController CreateController(){
            var controller = new AccountController(_userManager, _signInManager, _interaction.Object, _clientStore.Object, _schemeProvider.Object, _events.Object);
            return controller;
        }
    }
}