using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Test.Identity.Integration.RepositoryTests;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Bhbk.WebApi.Identity.User.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;

namespace Bhbk.Test.Identity.Integration.ControllerTests
{
    public class SessionControllerTests : IClassFixture<BaseUserControllerTests>
    {
        private readonly BaseUserControllerTests _factory;

        public SessionControllerTests(BaseUserControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_SessionV1_DeleteRefreshes_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new SessionController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.DeleteRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
                result = controller.DeleteRefreshesV1() as NotFoundObjectResult;
            }
        }

        [Fact]
        public void Me_SessionV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new SessionController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateUserRefreshes();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                    .Where(x => x.UserId == user.Id).ToLambda()).First();

                var result = controller.DeleteRefreshV1(refresh.Id) as OkObjectResult;
                result = controller.DeleteRefreshesV1() as OkObjectResult;
            }
        }

        [Fact]
        public void Me_SessionV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new SessionController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateUserRefreshes();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.GetRefreshesV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
            }
        }
    }
}
