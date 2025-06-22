using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class AuthCodeControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public AuthCodeControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUrls();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                var url = new Uri(_factory.TestData.Url.Link);

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

                HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(_factory.TestData.Url.Link);
                HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
            }
        }
    }
}
