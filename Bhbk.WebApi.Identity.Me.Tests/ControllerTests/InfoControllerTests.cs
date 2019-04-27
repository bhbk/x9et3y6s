using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.DeleteUserRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
                result = await controller.DeleteUserRefreshesV1() as NotFoundObjectResult;

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                for (int i = 0; i < 3; i++)
                    await JwtFactory.UserRefreshV2(_factory.UoW, issuer, user);

                var refresh = (await _factory.UoW.RefreshRepo.GetAsync(x => x.UserId == user.Id)).First();

                var result = await controller.DeleteUserRefreshV1(refresh.Id) as OkObjectResult;
                result = await controller.DeleteUserRefreshesV1() as OkObjectResult;

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.GetUserV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

                data.Id.Should().Be(user.Id);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var result = await controller.GetUserRefreshesV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<IEnumerable<RefreshModel>>().Subject;

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var model = new UserChangePassword()
                {
                    CurrentPassword = Strings.ApiUnitTestUserPassCurrent,
                    NewPassword = RandomValues.CreateBase64String(16),
                    NewPasswordConfirm = RandomValues.CreateBase64String(16)
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

                var check = await _factory.UoW.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var model = new UserChangePassword()
                {
                    CurrentPassword = Strings.ApiUnitTestUserPassCurrent,
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(model) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));

                var check = await _factory.UoW.UserRepo.CheckPasswordAsync(user.Id, model.NewPassword);
                check.Should().BeTrue();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_SetTwoFactor_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                controller.SetUser(user.Id);

                var status = await _factory.UoW.UserRepo.SetTwoFactorEnabledAsync(user.Id, false);
                status.Should().BeTrue();

                var result = await controller.SetTwoFactorV1(true) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Me_InfoV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var controller = new InfoController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

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

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
