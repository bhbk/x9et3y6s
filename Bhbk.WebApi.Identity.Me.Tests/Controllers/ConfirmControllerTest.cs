using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public async Task Api_Me_Confirm_ChangeEmail_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            string email = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestUserA;
            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var token = await TestIoC.UserMgmt.GenerateEmailConfirmationTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailChange(BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsAuhthorizationCodeLength),
                model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangeEmail_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            string email = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestUserA;
            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var token = await TestIoC.UserMgmt.GenerateEmailConfirmationTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailChange(token, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(16),
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var token = await TestIoC.UserMgmt.GeneratePasswordResetTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordChange(BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsPasswordLength),
                model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await TestIoC.UserMgmt.CheckPasswordAsync(user, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePassword_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = BaseLib.Statics.ApiUnitTestPasswordCurrent,
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var token = await TestIoC.UserMgmt.GeneratePasswordResetTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordChange(token, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await TestIoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePhone_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = string.Empty,
                NewPhoneNumberConfirm = string.Empty
            };

            var token = await TestIoC.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, model.CurrentPhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneChange(BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsAuhthorizationCodeLength),
                model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePhone_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            string phone = "32221110000";
            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = phone,
                NewPhoneNumberConfirm = phone
            };

            var token = await TestIoC.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, model.CurrentPhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneChange(token, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetEmail_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GenerateEmailConfirmationTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmail(user.Id,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsAuhthorizationCodeLength)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetEmail_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GenerateEmailConfirmationTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmail(user.Id, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GeneratePasswordResetTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPassword(user.Id,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsPasswordLength)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPassword_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GeneratePasswordResetTokenAsync(user);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPassword(user.Id, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPhoneNumber_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhone(user.Id,
                BaseLib.Helpers.CryptoHelper.CreateRandomBase64(TestIoC.ConfigMgmt.Tweaks.DefaultsAuhthorizationCodeLength)) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPhoneNumber_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var controller = new ConfirmController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var token = await TestIoC.UserMgmt.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhone(user.Id, token) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }
    }
}
