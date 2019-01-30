using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Mimoto.Exceptions;
using Mimoto.Quickstart.Account;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class ExternalControllerTest
    {

        private readonly FakeUserManager _userManager;
        private readonly FakeSignInManager _signInManager;
        private readonly Mock<IIdentityServerInteractionService> _interaction;
        private readonly Mock<IEventService> _events;
        public ExternalControllerTest()
        {
            _userManager = new FakeUserManager();
            _signInManager = new FakeSignInManager();
            _interaction = new Mock<IIdentityServerInteractionService>();
            _interaction.Setup(i => i.IsValidReturnUrl("http://localhost")).Returns(false);
            _interaction.Setup(i => i.IsValidReturnUrl("~/")).Returns(true);
            _events = new Mock<IEventService>();
        }

        [Fact]
        public async Task ChallengeShouldThrowInvalidReturnUrlException()
        {
            var controller = createController();

            Func<Task> action = async () =>
            {
                await controller.Challenge("test", "http://localhost");
            };

            await action.Should().ThrowAsync<InvalidReturnUrlException>();
        }

        [Fact]
        public async Task ChallengeShouldChallenge()
        {
            var controller = createController();

            var result = await controller.Challenge("test", null);

            result.Should().BeAssignableTo<ChallengeResult>();
            var challengeResult = result.As<ChallengeResult>();
            challengeResult.AuthenticationSchemes.Should().Contain("test");
            challengeResult.Properties.Items.Contains(new KeyValuePair<string, string>("returnUrl", "~/"));
            challengeResult.Properties.Items.Contains(new KeyValuePair<string, string>("scheme", "test"));
        }

        [Fact]
        public async Task CallbackShouldThrowException()
        {
            var controller = createController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = AuthenticatedHttpContext(AuthenticateResult.Fail(new Exception()))
            };

            Func<Task> func = async () => await controller.Callback();

            await func.Should().ThrowAsync<ExternalAuthenticationException>();
        }

        [Fact]
        public async Task CallbackFindShouldThrowException()
        {
            var controller = createController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = AuthenticatedHttpContext(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new System.Security.Claims.ClaimsPrincipal(),
                        new AuthenticationProperties(),
                        "test")
                    )
                )
            };

            Func<Task> func = async () => await controller.Callback();

            await func.Should().ThrowAsync<UnknownExternalUserIdException>();
        }

        [Fact]
        public async Task CallbackShouldProvisionUserAndRedirect()
        {
            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLoginSuccessEvent>()))
                .Returns(Task.CompletedTask);

            var controller = createController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = AuthenticatedHttpContext(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new System.Security.Claims.ClaimsPrincipal(
                            new ClaimsIdentity(new[] {
                                new Claim(JwtClaimTypes.Subject, "test1"),
                                new Claim(JwtClaimTypes.Name, "test1")
                            }
                        )),
                        new AuthenticationProperties(new Dictionary<string, string>(){
                            {"scheme","test"},
                            {"returnUrl","~/asdf"}
                        }),
                        "test")
                    )
                )
            };

            var result = await controller.Callback();

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/");
        }

        [Fact]
        public async Task CallbackShouldProvisionUserAndRedirectToReturnUrl()
        {
            _interaction.Setup(i => i.IsValidReturnUrl("~/asdf")).Returns(true);
            _events.Setup(e => e.RaiseAsync(It.IsAny<UserLoginSuccessEvent>()))
                .Returns(Task.CompletedTask);

            var controller = createController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = AuthenticatedHttpContext(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new System.Security.Claims.ClaimsPrincipal(
                            new ClaimsIdentity(new[] {
                                new Claim(JwtClaimTypes.Subject, "test1"),
                                new Claim(JwtClaimTypes.Name, "test1")
                            }
                        )),
                        new AuthenticationProperties(new Dictionary<string, string>(){
                            {"scheme","test"},
                            {"returnUrl","~/asdf"}
                        }),
                        "test")
                    )
                )
            };

            var result = await controller.Callback();

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("~/asdf");
        }

        // We cannot test Windows with Linux
        [Fact]
        public async Task ChallengeWindows()
        {
            _interaction.Setup(i => i.IsValidReturnUrl("~/")).Returns(true);

            var controller = createController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = AuthenticatedHttpContext(AuthenticateResult.Success(
                               new AuthenticationTicket(
                                   // We cannot test WindowsPrincipal under Linux
                                   // new WindowsPrincipal(WindowsIdentity.GetAnonymous()),
                                   new ClaimsPrincipal(),
                                   new AuthenticationProperties(new Dictionary<string, string>(){
                            {"scheme","test"},
                            {"returnUrl","~/asdf"}
                                   }),
                                   AccountOptions.WindowsAuthenticationSchemeName)
                               )
                           )
            };
            await controller.Challenge(AccountOptions.WindowsAuthenticationSchemeName, "~/");
        }

        private ExternalController createController()
        {
            var controller = new ExternalController(_userManager, _signInManager, _interaction.Object, _events.Object);
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("http://localhost")).Returns(false);
            url.Setup(u => u.IsLocalUrl("~/")).Returns(true);
            url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("Callback");
            controller.Url = url.Object;
            return controller;
        }

        private static HttpContext AuthenticatedHttpContext(AuthenticateResult result)
        {
            var httpContext = new DefaultHttpContext();

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.AuthenticateAsync(
                    It.IsAny<HttpContext>(), It.IsAny<string>())
                )
                .ReturnsAsync(result);

            var authSchemaProvider = new Mock<IAuthenticationSchemeProvider>();
            authSchemaProvider.Setup(a => a.GetDefaultAuthenticateSchemeAsync())
                .ReturnsAsync(
                    (new AuthenticationScheme("idp", "idp", typeof(IAuthenticationHandler))));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IdentityServerOptions)))
                .Returns(new IdentityServerOptions());
            serviceProvider.Setup(s => s.GetService(typeof(ISystemClock)))
                .Returns(new Mock<ISystemClock>().Object);
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationSchemeProvider)))
                .Returns(authSchemaProvider.Object);
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            serviceProvider.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(new Mock<ITempDataDictionaryFactory>().Object);
            httpContext.RequestServices = serviceProvider.Object;
            return httpContext;
        }
    }
}