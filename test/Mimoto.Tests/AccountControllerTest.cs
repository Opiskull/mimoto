using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Mimoto.Exceptions;
using Mimoto.Models;
using Mimoto.Quickstart.Account;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class AccountControllerTest
    {
        private readonly FakeUserManager _userManager;
        private readonly FakeSignInManager _signInManager;
        private readonly Mock<IIdentityServerInteractionService> _interaction;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IAuthenticationSchemeProvider> _schemeProvider;
        private readonly Mock<IEventService> _events;

        public AccountControllerTest()
        {
            _userManager = new FakeUserManager();
            _signInManager = new FakeSignInManager();
            _interaction = new Mock<IIdentityServerInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _schemeProvider = new Mock<IAuthenticationSchemeProvider>();
            _events = new Mock<IEventService>();
        }

        [Fact]
        public async Task LoginUserNamePasswordOkRedirectUrl(){
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            var controller = CreateController();

                
            var result = await controller.Login(new LoginInputModel{ 
                Username = "opi", 
                Password = "opi",
                ReturnUrl = "~/asdf"
                },"login");

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/asdf");            
        }

        [Fact]
        public async Task LoginUserNamePasswordOkReturnUrlEmpty(){
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            var controller = CreateController();
            var result = await controller.Login(new LoginInputModel{ 
                Username = "opi", 
                Password = "opi",
                ReturnUrl = ""
                },"login");

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/");            
        }

        [Fact]
        public async Task LoginUserNamePasswordOkThrowsException(){
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            var controller = CreateController();

            Func<Task> func = async () => {
                await controller.Login(new LoginInputModel{ 
                Username = "opi", 
                Password = "opi",
                ReturnUrl = "~/invalidUrl"
                },"login");
            };
            
            await func.Should().ThrowAsync<InvalidReturnUrlException>();
            
        }

        [Fact]
        public async Task LoginUserNamePasswordSuccessRedirectPkce(){
            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client{
                    Enabled = true,
                    RequirePkce = true
                });
            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLoginSuccessEvent>())).Returns(Task.CompletedTask).Verifiable();
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ClientId = "client1", });

            var controller = CreateController();
            var result = await controller.Login(new LoginInputModel{ 
                Username = "opi", 
                Password = "opi",
                ReturnUrl = "~/asdf"
                },"login");
            
            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().ViewName.Should().Be("Redirect");
            result.As<ViewResult>().Model.As<RedirectViewModel>().RedirectUrl.Should().Be("~/asdf");
            
            _events.Verify();
        }

        [Fact]
        public async Task LoginUserNamePasswordSuccessRedirect(){
            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLoginSuccessEvent>())).Returns(Task.CompletedTask).Verifiable();
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest());

            var controller = CreateController();
            var result = await controller.Login(new LoginInputModel{ 
                Username = "opi", 
                Password = "opi",
                ReturnUrl = "~/asdf"
                },"login");
            
            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url = "~/asdf";
            
            _events.Verify();
        }

        [Fact]
        public async Task LoginUserNamePasswordFailed(){
            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLoginFailureEvent>())).Returns(Task.CompletedTask).Verifiable();
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest());

            var controller = CreateController();
            var result = await controller.Login(new LoginInputModel{ 
                Username = "opi1", 
                Password = "opi"
                },"login");
            
            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().Model.Should().BeAssignableTo<LoginViewModel>();
            
            _events.Verify();
        }

        [Fact]
        public async Task LoginCancelButtonNoAuthorization(){
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            var controller = CreateController();

            var result = await controller.Login(new LoginInputModel(),"cancel");

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/");
        }

        [Fact]
        public async Task LoginCancelButton(){
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest());

            var controller = CreateController();

            var result = await controller.Login(new LoginInputModel{ ReturnUrl = "~/asdf"},"cancel");

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/asdf");
        }

        [Fact]
        public async Task LoginCancelButtonPkce(){
            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client{
                    Enabled = true,
                    RequirePkce = true
                });
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{
                    ClientId = "client1"
                });

            var controller = CreateController();

            var result = await controller.Login(new LoginInputModel{ ReturnUrl = "~/asdf"},"cancel");

            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().ViewName.Should().Be("Redirect");
            result.As<ViewResult>().Model.As<RedirectViewModel>().RedirectUrl = "~/asdf";
        }

        [Fact]
        public async Task LoginRedirectToExternal()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest
                {
                    ClientId = "client1",
                    ScopesRequested = new[] { "api1" },
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
                .ReturnsAsync(new AuthorizationRequest
                {
                    ClientId = "client1",
                    ScopesRequested = new[] { "api1" }
                });
            _schemeProvider.Setup(s => s.GetAllSchemesAsync()).ReturnsAsync(new[] {
                new AuthenticationScheme("test", "Test", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("test1", "Test 1", typeof(IAuthenticationHandler))
            });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client
                {
                    Enabled = true,
                    IdentityProviderRestrictions = new[] { "test", "test1" }
                });

            var controller = CreateController();
            var viewResult = await controller.Login("~/");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().Model.As<LoginViewModel>().ExternalProviders.Should().NotBeEmpty().And.HaveCount(2);
        }

        [Fact]
        public async Task Logout()
        {
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("sub","user1"),
                            new Claim(JwtClaimTypes.IdentityProvider, "testIdp")
                        }, "someAuth")
                    )
                }
            };

            var viewResult = await controller.Logout("blub");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().Model.As<LoggedOutViewModel>();
        }

        [Fact]
        public async Task LogoutWithoutUser()
        {

            _interaction.Setup(i => i.GetLogoutContextAsync("blub"))
                .ReturnsAsync(new LogoutRequest("iframeurl", new LogoutMessage()));
            var user = new Mock<IIdentity>();
            user.SetupGet(x => x.IsAuthenticated).Returns(false);
            user.SetupGet(x => x.Name).Returns("user1");

            var controller = CreateController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(user.Object)
                }
            };

            var viewResult = await controller.Logout("blub");
            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().Model.As<LoggedOutViewModel>();
        }

        [Fact]
        public async Task LogoutViewModelExternal()
        {
            var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("sub","user1"),
                            new Claim(JwtClaimTypes.IdentityProvider, "testIdp")
                        }, "someAuth")
                    )
                };

            var authServiceMock = new Mock<IAuthenticationHandlerProvider>();
            authServiceMock.Setup(a => a.GetHandlerAsync(
                    It.IsAny<HttpContext>(), It.IsAny<string>())
            )
                .ReturnsAsync(new Mock<IAuthenticationSignOutHandler>().Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationHandlerProvider)))
                .Returns(authServiceMock.Object);
            serviceProvider.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(new Mock<ITempDataDictionaryFactory>().Object);
            httpContext.RequestServices = serviceProvider.Object;

            _interaction.Setup(i => i.CreateLogoutContextAsync()).ReturnsAsync("blub");


            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLogoutSuccessEvent>()))
                .Returns(Task.CompletedTask);
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };

            var viewResult = await controller.Logout(new LogoutInputModel(){
                LogoutId = "blub"
            });
            viewResult.Should().BeAssignableTo<SignOutResult>();
            viewResult.As<SignOutResult>().Properties.RedirectUri.Should().Be("Callback");
            viewResult.As<SignOutResult>().AuthenticationSchemes.Should().Contain("testIdp");
        }

        private AccountController CreateController()
        {
            var controller = new AccountController(_userManager, _signInManager, _interaction.Object, _clientStore.Object, _schemeProvider.Object, _events.Object);
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("~/asdf")).Returns(true);
            url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("Callback");
            controller.Url = url.Object;
            return controller;
        }
    }
}