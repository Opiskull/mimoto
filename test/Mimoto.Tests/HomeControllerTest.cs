using System;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
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
        private Mock<IIdentityServerInteractionService> _identityService;
        private IHostingEnvironment _development;
        private IHostingEnvironment _production;
        private Mock<ILogger<HomeController>> _logger;

        public HomeControllerTest(){

            _identityService = new Mock<IIdentityServerInteractionService>();
            _identityService.Setup(i => i.GetErrorContextAsync("error-1"))
                .ReturnsAsync(new ErrorMessage{ ErrorDescription = "MyError"});
            _identityService.Setup(i => i.GetErrorContextAsync("wrong-id"))
                .ReturnsAsync((ErrorMessage)null);
            _logger = new Mock<ILogger<HomeController>>();
            
            _development = Mock.Of<IHostingEnvironment>(h => h.EnvironmentName == "Development");
            _production = Mock.Of<IHostingEnvironment>(h => h.EnvironmentName == "Production");
        }

        [Fact]
        public void IndexShouldReturnView()
        {
            var _controller = new HomeController(_identityService.Object,_development, _logger.Object);
            
            var viewResult = _controller.Index();

            viewResult.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void IndexShouldReturnNotFound()
        {
            var _controller = new HomeController(_identityService.Object,_production, _logger.Object);
            
            var viewResult = _controller.Index();

            viewResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ErrorShouldNotBeFound(){
            var _controller = new HomeController(_identityService.Object,_production, _logger.Object);
            
            var actionRsult = await _controller.Error("wrong-id");

            var viewResult = actionRsult.As<ViewResult>();
            var error = viewResult.ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().BeNull();
        }

        [Fact]
        public async Task ErrorDescriptionShouldBeNull(){
            var _controller = new HomeController(_identityService.Object,_production, _logger.Object);
            
            var actionRsult = await _controller.Error("error-1");

            var viewResult = actionRsult.As<ViewResult>();
            var error = viewResult.ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().NotBeNull();
            error.Error.ErrorDescription.Should().BeNull();
        }

        [Fact]
        public async Task ErrorDescriptionShouldBeMyError(){
            var _controller = new HomeController(_identityService.Object,_development, _logger.Object);
            
            var actionRsult = await _controller.Error("error-1");

            var viewResult = actionRsult.As<ViewResult>();
            var error = viewResult.ViewData.Model.As<ErrorViewModel>();

            error.Error.Should().NotBeNull();
            error.Error.ErrorDescription.Should().Be("MyError");
        }
    }
}
