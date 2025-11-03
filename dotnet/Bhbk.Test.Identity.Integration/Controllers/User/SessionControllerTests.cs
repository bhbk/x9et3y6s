using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Test.Identity.Integration.TestingTools;
using Bhbk.WebApi.Identity.User.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Controllers.User
{
    public class SessionControllerTests : IClassFixture<BaseUserControllerTests>
    {
        private readonly BaseUserControllerTests _factory;

        public SessionControllerTests(BaseUserControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_SessionV1_DeleteRefreshes_Fail()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateCredentialsTestSeed(uow);

            var controller = new SessionController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();

            controller.SetIdentity(issuer.Id, audience.Id, user.Id);

            var result = controller.DeleteRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
            result = controller.DeleteRefreshesV1() as NotFoundObjectResult;
        }

        [Fact]
        public void Me_SessionV1_DeleteRefreshes_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateSessionTestSeed(uow);

            var controller = new SessionController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();
            var refresh = seed.Refreshes.First();

            controller.SetIdentity(issuer.Id, audience.Id, user.Id);

            var result = controller.DeleteRefreshV1(refresh.Id) as OkObjectResult;
            result = controller.DeleteRefreshesV1() as OkObjectResult;
        }

        [Fact]
        public void Me_SessionV1_GetRefreshes_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateSessionTestSeed(uow);

            var controller = new SessionController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();

            controller.SetIdentity(issuer.Id, audience.Id, user.Id);

            var result = controller.GetRefreshesV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
        }
    }
}
