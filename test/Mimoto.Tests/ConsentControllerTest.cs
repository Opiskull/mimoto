using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mimoto.Quickstart.Consent;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class ConsentControllerTest{

        private readonly Mock<IIdentityServerInteractionService> _interactionService;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IResourceStore> _resourceStore;
        private readonly Mock<IEventService> _eventService;
        private readonly Mock<ILogger<ConsentController>> _logger;

        public ConsentControllerTest(){
            _interactionService = new Mock<IIdentityServerInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _resourceStore = new Mock<IResourceStore>();
            _eventService = new Mock<IEventService>();
            _logger = new Mock<ILogger<ConsentController>>();
        }

        [Fact]
        public async Task IndexShouldShowErrorAuthorization(){
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync((AuthorizationRequest)null);

            await ShouldShowError();
        }

        [Fact]
        public async Task IndexShouldShowErrorClient()
        {
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ ClientId = "client1"});

            _clientStore.Setup(c => c.FindEnabledClientByIdAsync("client1"))
                .ReturnsAsync((Client)null);

            await ShouldShowError();
        }

        [Fact]
        public async Task IndexShouldShowErrorResource(){
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ 
                    ClientId = "client1",
                    ScopesRequested = new [] { "api1"}
                });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client{
                    Enabled = true
                });

            _resourceStore.Setup(r => r.FindIdentityResourcesByScopeAsync(new [] { "api1"}))
                .ReturnsAsync(new IdentityResource [] {
                });
            
            _resourceStore.Setup(r => r.FindApiResourcesByScopeAsync(new [] { "api1"}))
                .ReturnsAsync(new ApiResource[] {
                });

            await ShouldShowError();
        }

        [Fact]
        public async Task ShouldDisplayConsentViewModel(){
            _interactionService.Setup(i => i.GetAuthorizationContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationRequest{ 
                    ClientId = "client1",
                    ScopesRequested = new [] { "api1"}
                });

            _clientStore.Setup(c => c.FindClientByIdAsync("client1"))
                .ReturnsAsync(new Client{
                    Enabled = true
                });

            _resourceStore.Setup(r => r.FindIdentityResourcesByScopeAsync(new [] { "api1"}))
                .ReturnsAsync(new [] { 
                    new IdentityResource { 
                        DisplayName = "Identity 1",
                        Name = "identity1"
                    }
                });
            
            _resourceStore.Setup(r => r.FindApiResourcesByScopeAsync(new [] { "api1"}))
                .ReturnsAsync(new [] { 
                    new ApiResource {
                        Scopes = new [] { 
                            new Scope {
                                Name = "api1"
                            }
                        }
                    }
                });

            var controller = new ConsentController(_interactionService.Object, _clientStore.Object, 
                    _resourceStore.Object, _eventService.Object, _logger.Object);

            var viewResult = await controller.Index("~/");

            viewResult.As<ViewResult>().ViewName.Should().Be("Index");
            viewResult.As<ViewResult>().Model.Should().BeAssignableTo<ConsentViewModel>();
        }

        private async Task ShouldShowError(){
            var controller = new ConsentController(_interactionService.Object, _clientStore.Object, 
                                    _resourceStore.Object, _eventService.Object, _logger.Object);

            var viewResult = await controller.Index("~/");

            viewResult.As<ViewResult>().ViewName.Should().Be("Error");
        }
    }
}