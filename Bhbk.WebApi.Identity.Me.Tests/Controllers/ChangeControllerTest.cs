using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [TestClass]
    public class ChangeControllerTest : StartupTest
    {
        private TestServer _owin;

        public ChangeControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Email_Fail()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1;

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = RandomValues.CreateBase64String(4),
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Email_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1;

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = newEmail,
                NewEmailConfirm = newEmail
            };

            var result = await controller.ChangeEmailV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Password_Fail()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = RandomValues.CreateBase64String(16),
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Password_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = Strings.ApiUnitTestUserPassCurrent,
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            var result = await controller.ChangePasswordV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Phone_Fail()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhone = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = newPhone,
                NewPhoneNumber = user.PhoneNumber,
                NewPhoneNumberConfirm = user.PhoneNumber
            };

            var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ChangeV1_Phone_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var controller = new ChangeController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhone = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
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
