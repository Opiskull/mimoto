using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Mimoto.Exceptions;
using Mimoto.Quickstart.Account;
using Moq;
using Xunit;

namespace Mimoto.Tests{
    public class ExternalControllerTest{

        private readonly FakeUserManager _userManager;
        private readonly FakeSignInManager _signInManager;
        private readonly Mock<IIdentityServerInteractionService> _interaction;
        private readonly Mock<IEventService> _events;
        public ExternalControllerTest(){
            _userManager = new FakeUserManager();
            _signInManager = new FakeSignInManager();
            _interaction = new Mock<IIdentityServerInteractionService>();
            _interaction.Setup(i => i.IsValidReturnUrl("http://localhost")).Returns(false);
            _interaction.Setup(i => i.IsValidReturnUrl("~/")).Returns(true);
            _events = new Mock<IEventService>();
        }

        [Fact]
        public async Task ChallengeShouldThrowInvalidReturnUrlException()
        {
            var controller = createController();

            Func<Task> action = async () => {
                await controller.Challenge("test", "http://localhost");
            };

            await action.Should().ThrowAsync<InvalidReturnUrlException>();
        }

        [Fact]
        public async Task ChallengeShouldChallenge(){
            var controller = createController();

            var result = await controller.Challenge("test",null);

            result.Should().BeAssignableTo<ChallengeResult>();
            var challengeResult = result.As<ChallengeResult>();
            challengeResult.AuthenticationSchemes.Should().Contain("test");
            challengeResult.Properties.Items.Contains(new KeyValuePair<string, string>("returnUrl","~/"));
            challengeResult.Properties.Items.Contains(new KeyValuePair<string, string>("scheme","test"));
        }

        private ExternalController createController(){
            var controller = new ExternalController(_userManager, _signInManager, _interaction.Object, _events.Object);
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("http://localhost")).Returns(false);
            url.Setup(u => u.IsLocalUrl("~/")).Returns(true);
            url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("Callback");
            controller.Url = url.Object;
            return controller;
        }
    }
}