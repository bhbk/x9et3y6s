using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tests.Controllers
{
    [TestClass]
    public class ConfirmControllerTest : StartupTest
    {
        private TestServer _owin;

        public ConfirmControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Email_Fail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Email_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newEmail = string.Format("{0}{1}", RandomValues.CreateBase64String(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newEmail, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Password_Fail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Password_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPassword = RandomValues.CreateBase64String(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(newPassword, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_PhoneNumber_Fail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                RandomValues.CreateBase64String(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_PhoneNumber_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ConfirmController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var newPhoneNumber = RandomValues.CreateNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }
    }
}
