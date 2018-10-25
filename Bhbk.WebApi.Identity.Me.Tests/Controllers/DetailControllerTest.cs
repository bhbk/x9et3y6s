using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [TestClass]
    public class DetailControllerTest : StartupTest
    {
        private TestServer _owin;

        public DetailControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Me_DetailV1_Get_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.GetDetailV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public void Api_Me_DetailV1_GetClaimList_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = controller.GetClaimsV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<Claim>>().Subject;
        }

        [TestMethod]
        public void Api_Me_DetailV1_GetQuoteOfDay_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = controller.GetQuoteOfDayV1() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserQuoteOfDay>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_DetailV1_SetPassword_Fail()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var model = new UserChangePassword()
            {
                CurrentPassword = Strings.ApiUnitTestUserPassCurrent,
                NewPassword = RandomValues.CreateBase64String(16),
                NewPasswordConfirm = RandomValues.CreateBase64String(16)
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _ioc.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Me_DetailV1_SetPassword_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var model = new UserChangePassword()
            {
                CurrentPassword = Strings.ApiUnitTestUserPassCurrent,
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _ioc.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_DetailV1_TwoFactor_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var status = await _ioc.UserMgmt.SetTwoFactorEnabledAsync(user, false);
            status.Should().BeAssignableTo(typeof(IdentityResult));
            status.Succeeded.Should().BeTrue();

            var result = await controller.SetTwoFactorV1(true) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_DetailV1_Update_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new DetailController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)"
            };

            var result = await controller.UpdateDetailV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
