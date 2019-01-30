using System;
using System.Linq;
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
using Mimoto.Quickstart.Device;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class DeviceControllerTest
    {
        private readonly Mock<IDeviceFlowInteractionService> _interaction;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IResourceStore> _resourceStore;
        private readonly Mock<IEventService> _events;
        private readonly Mock<ILogger<DeviceController>> _logger;

        public DeviceControllerTest()
        {
            _interaction = new Mock<IDeviceFlowInteractionService>();
            _clientStore = new Mock<IClientStore>();
            _resourceStore = new Mock<IResourceStore>();
            _events = new Mock<IEventService>();
            _logger = new Mock<ILogger<DeviceController>>();
        }

        [Fact]
        public async Task IndexShouldShowView()
        {
            var controller = createController();

            var viewResult = await controller.Index("");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("UserCodeCapture");
        }

        [Fact]
        public async Task IndexShouldShowError()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync((DeviceFlowAuthorizationRequest)null);

            var controller = createController();

            var viewResult = await controller.Index("userCode");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task UserCodeCaptureShouldShowError()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync((DeviceFlowAuthorizationRequest)null);

            var controller = createController();

            var viewResult = await controller.UserCodeCapture("userCode");

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task CallbackShouldThrowError()
        {
            var controller = createController();
            Func<Task> func = async () =>
            {
                await controller.Callback(null);
            };

            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CallbackShouldShowSuccess()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync((DeviceFlowAuthorizationRequest)null);

            var controller = createController();

            var viewResult = await controller.Callback(new DeviceAuthorizationInputModel());

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Success");
        }

        [Fact]
        public async Task CallbackShouldShowError()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync(new DeviceFlowAuthorizationRequest());

            var controller = createController();

            var viewResult = await controller.Callback(new DeviceAuthorizationInputModel
            {
                UserCode = "userCode"
            });

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task CallbackShouldProcessConsentNoButton()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync(new DeviceFlowAuthorizationRequest()).Verifiable();
            _interaction.Setup(i => i.HandleRequestAsync("userCode", ConsentResponse.Denied))
                .ReturnsAsync((DeviceFlowInteractionResult)null).Verifiable();
            _events.Setup(e => e.RaiseAsync(It.IsAny<ConsentDeniedEvent>()))
                .Returns(Task.CompletedTask).Verifiable();

            var controller = createController();

            var viewResult = await controller.Callback(new DeviceAuthorizationInputModel
            {
                UserCode = "userCode",
                Button = "no"
            });

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Success");

            _interaction.Verify();
            _events.Verify();
        }

        [Fact]
        public async Task CallbackShouldProcessConsentYesButtonError()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync(new DeviceFlowAuthorizationRequest()).Verifiable();

            var controller = createController();

            var viewResult = await controller.Callback(new DeviceAuthorizationInputModel
            {
                UserCode = "userCode",
                Button = "yes"
            });

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Error");

            _interaction.Verify();
            _events.Verify();
        }

        [Fact]
        public async Task CallbackShouldProcessConsentYesButton()
        {
            _interaction.Setup(i => i.GetAuthorizationContextAsync("userCode"))
                .ReturnsAsync(new DeviceFlowAuthorizationRequest{
                    ClientId = "client1"
                }).Verifiable();
            _interaction.Setup(i => i.HandleRequestAsync("userCode", It.Is<ConsentResponse>(
                (e) => e.RememberConsent == true && e.ScopesConsented.Contains("scope1") && 
                e.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))))
                .ReturnsAsync((DeviceFlowInteractionResult)null).Verifiable();
            _events.Setup(e => e.RaiseAsync(It.IsAny<ConsentGrantedEvent>()))
                .Returns(Task.CompletedTask).Verifiable();

            var controller = createController();

            var viewResult = await controller.Callback(new DeviceAuthorizationInputModel
            {
                RememberConsent = true,
                UserCode = "userCode",
                Button = "yes",
                ScopesConsented = new[] { "scope1", IdentityServerConstants.StandardScopes.OfflineAccess }
            });

            viewResult.Should().BeAssignableTo<ViewResult>();
            viewResult.As<ViewResult>().ViewName.Should().Be("Success");

            _interaction.Verify();
            _events.Verify();
        }

        private DeviceController createController()
        {
            var controller = new DeviceController(_interaction.Object, _clientStore.Object, _resourceStore.Object, _events.Object, _logger.Object);
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
    }
}