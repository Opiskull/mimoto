using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Mimoto.Quickstart;
using IdentityModel;
using FluentAssertions;

namespace Mimoto.Tests
{
    public class ExtensionsTest
    {
        [Fact]
        public void ShouldFindTestUserJwtClaimTypes()
        {
            var result = new[] {
                new Claim(JwtClaimTypes.GivenName, "test"),
                new Claim(JwtClaimTypes.FamilyName, "user"),
                new Claim(JwtClaimTypes.Email, "test@user.com")
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "test user"));
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Email, "test@user.com"));
        }

        [Fact]
        public void ShouldFindTestUserClaimTypes()
        {
            var result = new[]{
                new Claim(ClaimTypes.GivenName, "test1"),
                new Claim(ClaimTypes.Surname, "user1"),
                new Claim(ClaimTypes.Email, "test1@user1.com")
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "test1 user1"));
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Email, "test1@user1.com"));
        }

        [Fact]
        public void ShouldSetFirstName()
        {
            var result = new[]{
                new Claim(JwtClaimTypes.GivenName, "test"),
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "test"));
        }

        [Fact]
        public void ShouldSetLastName()
        {
            var result = new[]{
                new Claim(JwtClaimTypes.FamilyName, "user"),
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "user"));
        }

        [Fact]
        public void ShouldSetNameClaimTypes()
        {
            var result = new[]{
                new Claim(ClaimTypes.Name, "user"),
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "user"));
        }

        [Fact]
        public void ShouldSetNameJwtClaimTypes()
        {
            var result = new[]{
                new Claim(JwtClaimTypes.Name, "user"),
            }.GetNameAndEmailClaims();
            result.Should().ContainEquivalentOf(new Claim(JwtClaimTypes.Name, "user"));
        }
    }
}