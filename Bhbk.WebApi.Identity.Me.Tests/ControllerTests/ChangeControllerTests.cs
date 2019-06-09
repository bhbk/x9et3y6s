using AutoMapper;
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
using System.Threading.Tasks;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ChangeControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly BaseControllerTests _factory;

        public ChangeControllerTests(BaseControllerTests factory)
        {
            _factory = factory;
            _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var newEmail = Base64.CreateString(4) + "-" + FakeConstants.ApiTestUser;

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangeEmail()
                {
                    UserId = user.Id,
                    CurrentEmail = Base64.CreateString(4),
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var newEmail = Base64.CreateString(4) + "-" + FakeConstants.ApiTestUser;

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangeEmail()
                {
                    UserId = user.Id,
                    CurrentEmail = user.Email,
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangePassword()
                {
                    UserId = user.Id,
                    CurrentPassword = Base64.CreateString(16),
                    NewPassword = FakeConstants.ApiTestUserPassNew,
                    NewPasswordConfirm = FakeConstants.ApiTestUserPassNew
                };

                var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangePassword()
                {
                    UserId = user.Id,
                    CurrentPassword = FakeConstants.ApiTestUserPassCurrent,
                    NewPassword = FakeConstants.ApiTestUserPassNew,
                    NewPasswordConfirm = FakeConstants.ApiTestUserPassNew
                };

                var result = await controller.ChangePasswordV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Fail()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var newPhone = NumberAs.CreateString(10);

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangePhone()
                {
                    UserId = user.Id,
                    CurrentPhoneNumber = newPhone,
                    NewPhoneNumber = user.PhoneNumber,
                    NewPhoneNumberConfirm = user.PhoneNumber
                };

                var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Success()
        {
            var controller = new ChangeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).DestroyAsync().Wait();
                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var newPhone = NumberAs.CreateString(10);

                controller.SetUser(issuer.Id, user.Id);

                var model = new UserChangePhone()
                {
                    UserId = user.Id,
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
