using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class ChangeControllerTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;

        public ChangeControllerTests(StartupTests factory)
        {
            _factory = factory;
            _factory.CreateClient();
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Fail()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
            var newEmail = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser;

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                UserId = user.Id,
                CurrentEmail = RandomValues.CreateBase64String(4),
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Email_Success()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
            var newEmail = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestUser;

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                UserId = user.Id,
                CurrentEmail = user.Email,
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Fail()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                UserId = user.Id,
                CurrentPassword = RandomValues.CreateBase64String(16),
                NewPassword = Constants.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Constants.ApiUnitTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Password_Success()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                UserId = user.Id,
                CurrentPassword = Constants.ApiUnitTestUserPassCurrent,
                NewPassword = Constants.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Constants.ApiUnitTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Fail()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
            var newPhone = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                UserId = user.Id,
                CurrentPhoneNumber = newPhone,
                NewPhoneNumber = user.PhoneNumber,
                NewPhoneNumberConfirm = user.PhoneNumber
            };

            var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [Fact]
        public async Task Me_ChangeV1_Phone_Success()
        {
            var controller = new ChangeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
            var newPhone = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                UserId = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = newPhone,
                NewPhoneNumberConfirm = newPhone
            };

            var result = await controller.ChangePhoneV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }
    }
}
