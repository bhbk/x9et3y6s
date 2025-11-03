using AutoMapper;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Test.Identity.Integration.TestingTools;
using Bhbk.WebApi.Identity.User.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Controllers.User
{
    public class ProfileControllerTests : IClassFixture<BaseUserControllerTests>
    {
        private readonly BaseUserControllerTests _factory;

        public ProfileControllerTests(BaseUserControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_ProfileV1_Get_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateCredentialsTestSeed(uow);

            var controller = new ProfileController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();

            controller.SetIdentity(issuer.Id, audience.Id, user.Id);

            var result = controller.GetV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<UserV1>();
        }

        [Fact]
        public void Me_ProfileV1_Update_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var map = scope.ServiceProvider.GetRequiredService<IMapper>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateCredentialsTestSeed(uow);

            var controller = new ProfileController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();

            controller.SetIdentity(issuer.Id, audience.Id, user.Id);

            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = controller.UpdateV1(map.Map<UserV1>(user)) as OkObjectResult;
            var ok = result.Should().BeAssignableTo<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<UserV1>();
        }
    }
}
