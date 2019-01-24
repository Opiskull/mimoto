using System;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Mimoto.Tests
{
    public class AuthenticationBuilderTest
    {
        [Fact]
        public void Test1()
        {
            //    var whereMethods = typeof(AuthenticationBuilder)
            //     .GetMethods(BindingFlags.Static | BindingFlags.Public)
            //     .Where(mi => mi.Name == "Where"); 

            var authBuilder = new AuthenticationBuilder(new IServiceProvider());

            authBuilder.AddGoogle();
        }
    }
}
