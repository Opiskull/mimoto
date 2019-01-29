using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mimoto.Quickstart.Home;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class HomeControllerTest
    {
        private readonly Mock<IIdentityServerInteractionService> _identityService;
        private readonly IHostingEnvironment _development;
        private readonly IHostingEnvironment _production;
        private readonly Mock<ILogger<HomeController>> _logger;

        public HomeControllerTest()
        {

            _identityService = new Mock<IIdentityServerInteractionService>();
            _identityService.Setup(i => i.GetErrorContextAsync("error-1"))
                .ReturnsAsync(new ErrorMessage { ErrorDescription = "MyError" });
            _identityService.Setup(i => i.GetErrorContextAsync("wrong-id"))
                .ReturnsAsync((ErrorMessage)null);
            _logger = new Mock<ILogger<HomeController>>();

            _development = Mock.Of<IHostingEnvironment>(h => h.EnvironmentName == "Development");
            _production = Mock.Of<IHostingEnvironment>(h => h.EnvironmentName == "Production");
        }

        [Fact]
        public void IndexShouldReturnView()
        {
            var controller = new HomeController(_identityService.Object, _development, _logger.Object);

            var viewResult = controller.Index();

            viewResult.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void IndexShouldReturnNotFound()
        {
            var controller = new HomeController(_identityService.Object, _production, _logger.Object);

            var viewResult = controller.Index();

            viewResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ErrorShouldNotBeFound()
        {
            var controller = new HomeController(_identityService.Object, _production, _logger.Object);

            var actionRsult = await controller.Error("wrong-id");

            var error = actionRsult.As<ViewResult>().ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().BeNull();
        }

        [Fact]
        public async Task ErrorDescriptionShouldBeNull()
        {
            var controller = new HomeController(_identityService.Object, _production, _logger.Object);

            var actionRsult = await controller.Error("error-1");

            var error = actionRsult.As<ViewResult>().ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().NotBeNull();
            error.Error.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public async Task ErrorDescriptionShouldBeMyError()
        {
            var controller = new HomeController(_identityService.Object, _development, _logger.Object);

            var actionRsult = await controller.Error("error-1");

            var error = actionRsult.As<ViewResult>().ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().NotBeNull();
            error.Error.ErrorDescription.Should().Be("MyError");
        }
    }
}
