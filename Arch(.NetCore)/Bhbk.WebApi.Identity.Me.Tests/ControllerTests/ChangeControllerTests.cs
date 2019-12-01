using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ChangeControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public ChangeControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_ChangeV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newEmail = Base64.CreateString(4) + "-" + FakeConstants.ApiTestUser;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeModel()
                {
                    EntityId = user.Id,
                    CurrentEmail = Base64.CreateString(4),
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = controller.ChangeEmailV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ChangeV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newEmail = Base64.CreateString(4) + "-" + FakeConstants.ApiTestUser;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeModel()
                {
                    EntityId = user.Id,
                    CurrentEmail = user.Email,
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = controller.ChangeEmailV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public void Me_ChangeV1_Password_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeModel()
                {
                    EntityId = user.Id,
                    CurrentPassword = Base64.CreateString(16),
                    NewPassword = FakeConstants.ApiTestUserPassNew,
                    NewPasswordConfirm = FakeConstants.ApiTestUserPassNew
                };

                var result = controller.ChangePasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ChangeV1_Password_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeModel()
                {
                    EntityId = user.Id,
                    CurrentPassword = FakeConstants.ApiTestUserPassCurrent,
                    NewPassword = FakeConstants.ApiTestUserPassNew,
                    NewPasswordConfirm = FakeConstants.ApiTestUserPassNew
                };

                var result = controller.ChangePasswordV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public void Me_ChangeV1_Phone_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPhone = NumberAs.CreateString(10);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeModel()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = newPhone,
                    NewPhoneNumber = user.PhoneNumber,
                    NewPhoneNumberConfirm = user.PhoneNumber
                };

                var result = controller.ChangePhoneV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_ChangeV1_Phone_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ChangeController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == FakeConstants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();
                var newPhone = NumberAs.CreateString(10);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeModel()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = user.PhoneNumber,
                    NewPhoneNumber = newPhone,
                    NewPhoneNumberConfirm = newPhone
                };

                var result = controller.ChangePhoneV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }
    }
}
