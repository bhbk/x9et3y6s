using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Me.Controller;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;
using Bhbk.Lib.Identity.Factory;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    [TestClass]
    public class ConfirmControllerTest : BaseControllerTest
    {
        [TestMethod]
        public async Task Api_Me_Confirm_ChangeEmail_Fail()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var token = await UoW.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailChange(model,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultAuhthorizationCodeLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangeEmail_Success()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var token = await UoW.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmailChange(model, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePassword_Fail()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordChange(model,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultPasswordLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            var check = await UoW.UserMgmt.CheckPasswordAsync(user, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePassword_Success()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = BaseLib.Statics.ApiUnitTestPasswordCurrent,
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPasswordChange(model, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await UoW.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePhone_Fail()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = string.Empty,
                NewPhoneNumberConfirm = string.Empty
            };

            var token = await UoW.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, model.CurrentPhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneChange(model,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultAuhthorizationCodeLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_ChangePhone_Success()
        {
            string phone = "32221110000";
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = phone,
                NewPhoneNumberConfirm = phone
            };

            var token = await UoW.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, model.CurrentPhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhoneChange(model, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetEmail_Fail()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmail(user.Id,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultAuhthorizationCodeLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetEmail_Success()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GenerateEmailConfirmationTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmEmail(user.Id, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPassword_Fail()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPassword(user.Id,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultPasswordLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPassword_Success()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GeneratePasswordResetTokenAsync(user.Id);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPassword(user.Id, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPhoneNumber_Fail()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, user.PhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhone(user.Id,
                EntrophyHelper.GenerateRandomBase64(UoW.ConfigMgmt.Tweaks.DefaultAuhthorizationCodeLength)) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Confirm_SetPhoneNumber_Success()
        {
            var controller = new ConfirmController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var token = await UoW.UserMgmt.GenerateChangePhoneNumberTokenAsync(user.Id, user.PhoneNumber);
            token.Should().NotBeNullOrEmpty();

            var result = await controller.ConfirmPhone(user.Id, token) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }
    }
}
