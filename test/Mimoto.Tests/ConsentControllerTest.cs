using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mimoto.Quickstart.Account;
using Mimoto.Quickstart.Consent;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class ConsentControllerTest
    {

        private readonly Mock<IIdentityServerInteractionService> _interactionService;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IResourceStore> _resourceStore;
        private readonly Mock<IEventService> _eventService;
        private readonly Mock<ILogger<ConsentController>> _logger;

        private readonly Client _client = new Client
        {
            Enabled = true,
            ClientId = "client1"
        };

        private readonly AuthorizationRequest _authorizationRequest = new AuthorizationRequest
        {
            ClientId = "client1",
            ScopesRequested = new[] { "api1", IdentityServerConstants.StandardScopes.OfflineAccess }
        };

        public ConsentControllerTest()
        {
            _interactionService = new Mock<IIdentityServerInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _resourceStore = new Mock<IResourceStore>();
            _eventService = new Mock<IEventService>();
            _logger = new Mock<ILogger<ConsentController>>();
        }

        [Fact]
        public async Task IndexShouldShowErrorAuthorization()
        {
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            await ShouldShowError();
        }

        [Fact]
        public async Task IndexShouldShowErrorClient()
        {
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest { ClientId = "client1" });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync((Client)null);

            await ShouldShowError();
        }

        [Fact]
        public async Task IndexShouldShowErrorResource()
        {
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(_authorizationRequest);

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(_client);

            _resourceStore.Setup(r => r.FindIdentityResourcesByScopeAsync(new[] { "api1" }))
                .ReturnsAsync(new IdentityResource[] {
                });

            _resourceStore.Setup(r => r.FindApiResourcesByScopeAsync(new[] { "api1" }))
                .ReturnsAsync(new ApiResource[] {
                });

            await ShouldShowError();
        }

        [Fact]
        public async Task ShouldDisplayConsentViewModel()
        {
            var scopes = _authorizationRequest.ScopesRequested;
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(_authorizationRequest);

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(_client);

            _resourceStore.Setup(r => r.FindIdentityResourcesByScopeAsync(scopes))
                .ReturnsAsync(new[] {
                    new IdentityResource {
                        DisplayName = "Identity 1",
                        Name = "identity1"
                    }
                });

            _resourceStore.Setup(r => r.FindApiResourcesByScopeAsync(scopes))
                .ReturnsAsync(new[] {
                    new ApiResource {
                        Scopes = new [] {
                            new Scope {
                                Name = "api1"
                            }
                        }
                    }
                });

            var controller = CreateController();

            var viewResult = await controller.Index("~/");

            viewResult.As<ViewResult>().ViewName.Should().Be("Index");
            viewResult.As<ViewResult>().Model.Should().BeAssignableTo<ConsentViewModel>();
        }

        [Fact]
        public async Task IndexShouldShowError()
        {
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            var controller = CreateController();

            var result = await controller.Index(new ConsentInputModel());

            result.As<ViewResult>().ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task ProcessConsentShouldBeNo()
        {
            var consentModel = new ConsentInputModel();
            consentModel.Button = "no";
            consentModel.ReturnUrl = "returnUrl";
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(_authorizationRequest);

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client
                {
                    Enabled = true,
                    RequirePkce = true
                });

            _eventService.Setup(e => e.RaiseAsync(It.IsAny<ConsentDeniedEvent>()))
                .Returns(Task.CompletedTask).Verifiable();

            _interactionService.Setup(i => i.GrantConsentAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<ConsentResponse>(), null))
                .Returns(Task.CompletedTask);

            var controller = CreateController();
            var result = await controller.Index(consentModel);

            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().Model.Should().BeAssignableTo<RedirectViewModel>();
            result.As<ViewResult>().Model.As<RedirectViewModel>().RedirectUrl.Should().Be("returnUrl");
        }

        [Fact]
        public async Task ProcessConsentInvalid()
        {
            var consentModel = new ConsentInputModel();
            consentModel.Button = "invalid";
            consentModel.ReturnUrl = "returnUrl";
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest
                {
                    ClientId = "client1",
                    ScopesRequested = new[] { "api1" }
                });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(_client);

            _resourceStore.Setup(r => r.FindIdentityResourcesByScopeAsync(new[] { "api1" }))
                .ReturnsAsync(new[] {
                    new IdentityResource {
                        DisplayName = "Identity 1",
                        Name = "identity1"
                    }
                });

            _resourceStore.Setup(r => r.FindApiResourcesByScopeAsync(new[] { "api1" }))
                .ReturnsAsync(new[] {
                    new ApiResource {
                        Scopes = new [] {
                            new Scope {
                                Name = "api1"
                            }
                        }
                    }
                });

            var controller = CreateController();
            var result = await controller.Index(consentModel);

            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().ViewName.Should().Be("Index");
            result.As<ViewResult>().Model.Should().BeAssignableTo<ConsentViewModel>();
            controller.ModelState.ErrorCount.Should().Be(1);
        }

        [Fact]
        public async Task ProcessConsentShouldBeYesGranted()
        {
            var consentModel = new ConsentInputModel();
            consentModel.Button = "yes";
            consentModel.ReturnUrl = "returnUrl";
            consentModel.ScopesConsented = new[] { "api1" };
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(_authorizationRequest);

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(_client);

            _eventService.Setup(e => e.RaiseAsync(It.IsAny<ConsentGrantedEvent>()))
                .Returns(Task.CompletedTask).Verifiable();

            _interactionService.Setup(i => i.GrantConsentAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<ConsentResponse>(), null))
                .Returns(Task.CompletedTask);

            var controller = CreateController();
            var result = await controller.Index(consentModel);

            result.Should().BeAssignableTo<RedirectResult>();
            result.As<RedirectResult>().Url.Should().Be("returnUrl");
            _eventService.Verify();
        }

        [Fact]
        public async Task ProcessConsentShouldBeYesFailed()
        {
            var consentModel = new ConsentInputModel();
            consentModel.Button = "yes";
            consentModel.ReturnUrl = "returnUrl";
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(_authorizationRequest);

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(_client);

            _interactionService.Setup(i => i.GrantConsentAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<ConsentResponse>(), null))
                .Returns(Task.CompletedTask);

            var controller = CreateController();
            var result = await controller.Index(consentModel);

            result.Should().BeAssignableTo<ViewResult>();
            result.As<ViewResult>().ViewName.Should().Be("Error");
        }

        private ConsentController CreateController()
        {
            var controller = new ConsentController(_interactionService.Object, _clientStore.Object,
                                    _resourceStore.Object, _eventService.Object, _logger.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("sub","user1")
                        })
                    )
                }
            };
            return controller;
        }

        private async Task ShouldShowError()
        {
            var controller = CreateController();

            var viewResult = await controller.Index("~/");

            viewResult.As<ViewResult>().ViewName.Should().Be("Error");
        }
    }
}