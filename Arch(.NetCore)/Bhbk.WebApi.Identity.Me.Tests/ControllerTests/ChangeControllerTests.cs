using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ChangeControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public ChangeControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public async ValueTask Me_ChangeV1_Email_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow);
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();
                var newEmail = Base64.CreateString(4) + "-" + TestDefaultConstants.UserName;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeV1()
                {
                    EntityId = user.Id,
                    CurrentEmail = Base64.CreateString(4),
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_ChangeV1_Email_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();
                var newEmail = Base64.CreateString(4) + "-" + TestDefaultConstants.UserName;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeV1()
                {
                    EntityId = user.Id,
                    CurrentEmail = user.UserName,
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_ChangeV1_Password_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPassword = Base64.CreateString(16),
                    NewPassword = TestDefaultConstants.UserPassNew,
                    NewPasswordConfirm = TestDefaultConstants.UserPassNew
                };

                var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_ChangeV1_Password_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPassword = TestDefaultConstants.UserPassCurrent,
                    NewPassword = TestDefaultConstants.UserPassNew,
                    NewPasswordConfirm = TestDefaultConstants.UserPassNew
                };

                var result = await controller.ChangePasswordV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_ChangeV1_Phone_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();
                var newPhone = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = newPhone,
                    NewPhoneNumber = user.PhoneNumber,
                    NewPhoneNumberConfirm = user.PhoneNumber
                };

                var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_ChangeV1_Phone_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new ChangeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();
                var newPhone = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = user.PhoneNumber,
                    NewPhoneNumber = newPhone,
                    NewPhoneNumberConfirm = newPhone
                };

                var result = await controller.ChangePhoneV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }
    }
}
