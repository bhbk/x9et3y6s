﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TBL;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_TBL;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
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
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory_TBL(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUrls();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var url = new Uri(Constants.TestUriLink);

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

                HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(Constants.TestUriLink);
                HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
            }
        }
    }
}
