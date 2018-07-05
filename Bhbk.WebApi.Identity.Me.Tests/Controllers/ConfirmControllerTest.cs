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
using BaseLib = Bhbk.Lib.Identity;

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
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newEmail = string.Format("{0}{1}", BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(newEmail, TimeSpan.FromSeconds(10), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Email_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newEmail = string.Format("{0}{1}", BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4), user.Email);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(newEmail, TimeSpan.FromSeconds(10), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Password_Fail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newPassword = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(newPassword, TimeSpan.FromSeconds(10), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_Password_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newPassword = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(12);

            controller.SetUser(user.Id);

            var token = await new ProtectProvider(_ioc.ContextStatus.ToString()).GenerateAsync(newPassword, TimeSpan.FromSeconds(10), user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_PhoneNumber_Fail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newPhoneNumber = BaseLib.Helpers.CryptoHelper.CreateRandomNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(token.Length)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_ConfirmV1_PhoneNumber_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ConfirmController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var newPhoneNumber = BaseLib.Helpers.CryptoHelper.CreateRandomNumberAsString(10);

            controller.SetUser(user.Id);

            var token = await new TotpProvider(8, 10).GenerateAsync(newPhoneNumber, user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }
    }
}
