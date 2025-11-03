using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Test.Identity.Integration.TestingTools;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Web;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Controllers.Sts
{
    public class AuthCodeControllerTests : IClassFixture<BaseStsControllerTests>
    {
        private readonly BaseStsControllerTests _factory;

        public AuthCodeControllerTests(BaseStsControllerTests factory) => _factory = factory;

        [Fact]
        public void Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateOAuthAuthCodeSeed(uow);

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();
            var urlEntity = seed.Urls.Values.First();
            var url = new Uri(urlEntity.UrlHost + urlEntity.UrlPath);

            var ask = controller.AuthCodeV2_Ask(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = audience.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);

            HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(url.AbsoluteUri);
            HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
        }
    }
}
