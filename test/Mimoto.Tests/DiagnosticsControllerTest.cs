using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Mimoto.Quickstart.Diagnostics;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class DiagnosticsControllerTest
    {

        private readonly HttpContext _localContext;
        private readonly HttpContext _remoteContext;

        public DiagnosticsControllerTest()
        {
            _localContext = new DefaultHttpContext { };
            _localContext.Connection.LocalIpAddress = IPAddress.Loopback;
            _localContext.Connection.RemoteIpAddress = IPAddress.Loopback;
            _remoteContext = new DefaultHttpContext();
            _remoteContext.Connection.LocalIpAddress = IPAddress.Loopback;
            _remoteContext.Connection.RemoteIpAddress = IPAddress.Parse("127.1.1.1");
        }

        [Fact]
        public async Task IndexShouldReturnNotFound()
        {
            var controller = new DiagnosticsController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _remoteContext
            };
            var notFoundResult = await controller.Index();
            notFoundResult.As<NotFoundResult>().Should().NotBeNull();
        }

        [Fact]
        public async Task IndexShouldReturnDiganosticsViewModel()
        {
            var controller = new DiagnosticsController();
            var authProp = new AuthenticationProperties(
                new Dictionary<string, string> {
                    {"client_list", Base64Url.Encode(Encoding.UTF8.GetBytes("[\"client1\"]"))}
                }
            );

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.AuthenticateAsync(
                    It.IsAny<HttpContext>(), It.IsAny<string>())
                )
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new System.Security.Claims.ClaimsPrincipal(),
                        authProp,
                        "test"
                    )
                ));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            serviceProvider.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(new Mock<ITempDataDictionaryFactory>().Object);
            _localContext.RequestServices = serviceProvider.Object;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _localContext
            };

            var viewModel = await controller.Index();

            var diagVM = viewModel.As<ViewResult>().ViewData.Model.As<DiagnosticsViewModel>();
            diagVM.Should().NotBeNull();
            diagVM.AuthenticateResult.Should().NotBeNull();
            diagVM.Clients.Should().Contain("client1");
        }
    }
}