using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;

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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new AuthCodeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

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

                HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(FakeConstants.ApiTestUriLink);
                HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
            }
        }
    }
}
