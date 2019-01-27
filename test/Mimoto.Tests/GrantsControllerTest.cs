
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimoto.Quickstart.Grants;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class GrantsControllerTest
    {
        private readonly Mock<IIdentityServerInteractionService> _interactionService;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IResourceStore> _resourceStore;
        private readonly Mock<IEventService> _eventService;

        public GrantsControllerTest(){
            _interactionService = new Mock<IIdentityServerInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _clientStore.Setup(c => c.FindClientByIdAsync("client1")).ReturnsAsync(new Client{
                ClientId = "client1",
                ClientName = "Client 1"
            });
            _resourceStore = new Mock<IResourceStore>();
            _eventService = new Mock<IEventService>();
        }

        [Fact]
        public async Task RevokeConsentFireEventWithRedirect(){
            _interactionService.Setup(i => i.RevokeUserConsentAsync("client1"))
                .Returns(Task.CompletedTask).Verifiable();
            _eventService.Setup(e => e.RaiseAsync(new GrantsRevokedEvent("user1","client1")))
                .Returns(Task.CompletedTask).Verifiable();
            var controller = new GrantsController(_interactionService.Object, _clientStore.Object, _resourceStore.Object, _eventService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { 
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name,"user1")
                        })
                    )
                }
            };

            var result = await controller.Revoke("client1");

            result.As<RedirectToActionResult>().ActionName.Should().Be("Index");
            _interactionService.Verify();
            _eventService.Verify();
        }

        [Fact]
        public async Task IndexShouldReturnEmptyModel(){      
            _interactionService.Setup(i => i.GetAllUserConsentsAsync())
                .ReturnsAsync(new Consent[]{});     

            var controller = new GrantsController(_interactionService.Object, _clientStore.Object, _resourceStore.Object, _eventService.Object);

            var viewResult = await controller.Index();

            var grantsModel = viewResult.As<ViewResult>().ViewData.Model.As<GrantsViewModel>();

            grantsModel.Grants.Should().NotBeNull();
            grantsModel.Grants.Should().BeEmpty();
        }


        // [Fact]
        // public async Task IndexShouldReturn2Models(){      
        //     _interactionService.Setup(i => i.GetAllUserConsentsAsync())
        //         .ReturnsAsync(new Consent[]{
        //             new Consent {
        //                 ClientId = "client1",
        //                 Scopes = new [] {"api1"}
        //             },
        //             new Consent {
        //                 ClientId = "client2",
        //                 Scopes = new [] {"api2"}
        //             }
        //         });
            
        //     _resourceStore.Setup(r => r.FindResourcesByScopeAsync(It.IsAny<IEnumerable<string>>()))
        //         .ReturnsAsync(new Resources {
        //             ApiResources = new [] { 
        //                 new ApiResource { DisplayName = "Api 1"}},
        //             IdentityResources = new [] { 
        //                 new IdentityResource { DisplayName = "Identity 1"}}
        //         });

        //     var controller = new GrantsController(_interactionService.Object, _clientStore.Object, _resourceStore.Object, _eventService.Object);

        //     var viewResult = await controller.Index();

        //     var grantsModel = viewResult.As<ViewResult>().ViewData.Model.As<GrantsViewModel>();

        //     grantsModel.Grants.Should().NotBeNull();
        //     grantsModel.Grants.Should().NotBeEmpty().And.HaveCount(1);
        // }
    }
}