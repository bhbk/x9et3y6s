using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Me;
using Bhbk.Lib.Identity.Internal.Datasets;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [Collection("MeTests")]
    public class DetailControllerTest
    {
        private readonly StartupTest _factory;

        public DetailControllerTest(StartupTest factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Me_DetailV1_GetMotD_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                controller.SetUser(user.Id);

                var result = controller.GetQuoteOfTheDayV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserQuotes>().Subject;
            }
        }

        [Fact]
        public async Task Me_DetailV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                controller.SetUser(user.Id);

                var result = await controller.GetDetailV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

                data.Id.Should().Be(user.Id);
            }
        }

        [Fact]
        public async Task Me_DetailV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
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
            }
        }

        [Fact]
        public async Task Me_DetailV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
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
            }
        }

        [Fact]
        public async Task Me_DetailV1_TwoFactor_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                controller.SetUser(user.Id);

                var status = await _factory.UoW.UserRepo.SetTwoFactorEnabledAsync(user.Id, false);
                status.Should().BeTrue();

                var result = await controller.SetTwoFactorV1(true) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));
            }
        }

        [Fact]
        public async Task Me_DetailV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await new GenerateTestData(_factory.UoW).DestroyAsync();
                await new GenerateTestData(_factory.UoW).CreateAsync();

                var controller = new DetailController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

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

                var result = await controller.UpdateDetailV1(model) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeAssignableTo<UserModel>().Subject;

                data.FirstName.Should().Be(model.FirstName);
                data.LastName.Should().Be(model.LastName);
            }
        }
    }
}
