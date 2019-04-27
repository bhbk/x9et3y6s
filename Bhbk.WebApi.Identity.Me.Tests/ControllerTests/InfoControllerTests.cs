using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    [Collection("MeTests")]
    public class InfoControllerTests
    {
        private readonly StartupTests _factory;

        public InfoControllerTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Me_InfoV1_DeleteRefreshes_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.DeleteUserRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
                result = await controller.DeleteUserRefreshesV1() as NotFoundObjectResult;
            }
        }

        [Fact]
        public async Task Me_InfoV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                for (int i = 0; i < 3; i++)
                    await JwtFactory.UserRefreshV2(uow, issuer, user);

                var refresh = (await uow.RefreshRepo.GetAsync(x => x.UserId == user.Id)).First();

                var result = await controller.DeleteUserRefreshV1(refresh.Id) as OkObjectResult;
                result = await controller.DeleteUserRefreshesV1() as OkObjectResult;
            }
        }

        [Fact]
        public async Task Me_InfoV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.GetUserV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

                data.Id.Should().Be(user.Id);
            }
        }

        [Fact]
        public async Task Me_InfoV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.GetUserRefreshesV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<IEnumerable<RefreshModel>>().Subject;
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var model = new UserChangePassword()
                {
                    CurrentPassword = Constants.ApiUnitTestUserPassCurrent,
                    NewPassword = RandomValues.CreateBase64String(16),
                    NewPasswordConfirm = RandomValues.CreateBase64String(16)
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

                var check = await uow.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var model = new UserChangePassword()
                {
                    CurrentPassword = Constants.ApiUnitTestUserPassCurrent,
                    NewPassword = Constants.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Constants.ApiUnitTestUserPassNew
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(model) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));

                var check = await uow.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetTwoFactor_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var status = await uow.UserRepo.SetTwoFactorEnabledAsync(user.Id, false);
                status.Should().BeTrue();

                var result = await controller.SetTwoFactorV1(true) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));
            }
        }

        [Fact]
        public async Task Me_InfoV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var model = new UserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName + "(Updated)",
                    LastName = user.LastName + "(Updated)",
                    HumanBeing = false,
                    Immutable = false,
                };

                var result = await controller.UpdateUserV1(model) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

                data.FirstName.Should().Be(model.FirstName);
                data.LastName.Should().Be(model.LastName);
            }
        }
    }
}
