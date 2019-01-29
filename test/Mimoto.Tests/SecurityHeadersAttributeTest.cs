using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Mimoto.Quickstart;
using Moq;
using Xunit;

namespace Mimoto.Tests
{
    public class SecurityHeadersAttrbuteTest
    {

        public SecurityHeadersAttrbuteTest()
        {

        }

        [Fact]
        public void ShouldAddSecureHeaders()
        {
            var securityHeaders = new SecurityHeadersAttribute();

            var resultExecutingContext = new ResultExecutingContext(
                new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor(),
                },
                new Mock<IList<IFilterMetadata>>().Object,
                new ViewResult(),
                new object());

            resultExecutingContext.HttpContext = new DefaultHttpContext();

            securityHeaders.OnResultExecuting(resultExecutingContext);

            var responseHeaders = resultExecutingContext.HttpContext.Response.Headers;

            responseHeaders.Should().Contain("X-Content-Type-Options", "nosniff");
            responseHeaders.Should().Contain("X-Frame-Options", "SAMEORIGIN");
            responseHeaders.Should().Contain("Content-Security-Policy",
                "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';");
            responseHeaders.Should().Contain("X-Content-Security-Policy",
                "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';");
            responseHeaders.Should().Contain("Referrer-Policy", "no-referrer");
        }
    }
}