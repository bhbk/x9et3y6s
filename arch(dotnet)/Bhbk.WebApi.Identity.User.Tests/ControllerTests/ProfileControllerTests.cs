using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.WebApi.Identity.User.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;

namespace Bhbk.WebApi.Identity.User.Tests.ControllerTests
{
    public class ProfileControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public ProfileControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_ProfileV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ProfileController();
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

                var result = controller.GetV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<UserV1>();
            }
        }

        [Fact]
        public void Me_ProfileV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ProfileController();
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

                user.FirstName += "(Updated)";
                user.LastName += "(Updated)";

                var result = controller.UpdateV1(map.Map<UserV1>(user)) as OkObjectResult;
                var ok = result.Should().BeAssignableTo<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<UserV1>();
            }
        }
    }
}
