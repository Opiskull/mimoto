using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
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

        public DeviceControllerTest(){
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

        private DeviceController createController(){
            var controller = new DeviceController(_interaction.Object, _clientStore.Object, _resourceStore.Object, _events.Object, _logger.Object);
            return controller;
        }
    }
}