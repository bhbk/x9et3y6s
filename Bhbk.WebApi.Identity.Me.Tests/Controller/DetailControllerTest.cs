using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helper;
using Bhbk.WebApi.Identity.Me.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    [TestClass]
    public class DetailControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public DetailControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangeEmail_Fail()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                NewEmail = email,
                NewEmailConfirm = email
            };

            var result = await controller.AskChangeEmail(model) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));

            //var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var result = await _owin.HttpClient.PutAsync("/detail/v1/" + user.Id.ToString() + "/change-password", content);
            //result.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangeEmail_Success()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var result = await controller.AskChangeEmail(model) as OkNegotiatedContentResult<string>;
            result.Content.Should().BeAssignableTo(typeof(string));

            //var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var result = await _owin.HttpClient.PutAsync("/detail/v1/" + user.Id.ToString() + "/change-password", content);
            //result.Content.Should().BeAssignableTo(typeof(string));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePassword_Fail()
        {
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AskChangePassword(model) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePassword_Success()
        {
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = BaseLib.Statics.ApiUnitTestPasswordCurrent,
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AskChangePassword(model) as OkNegotiatedContentResult<string>;
            result.Content.Should().BeAssignableTo(typeof(string));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePhone_Fail()
        {
            string phone = "01112223333";
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = phone,
                NewPhoneNumber = user.PhoneNumber,
                NewPhoneNumberConfirm = user.PhoneNumber
            };

            var result = await controller.AskChangePhone(model) as BadRequestErrorMessageResult;
            result.Should().BeAssignableTo(typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePhone_Success()
        {
            string phone = "01112223333";
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = phone,
                NewPhoneNumberConfirm = phone
            };

            var result = await controller.AskChangePhone(model) as OkNegotiatedContentResult<string>;
            result.Content.Should().BeAssignableTo(typeof(string));
        }

        [TestMethod]
        public void Api_Me_Detail_GetClaimList_Success()
        {
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var result = controller.GetClaims() as OkNegotiatedContentResult<IEnumerable<Claim>>;
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<Claim>));
        }

        [TestMethod]
        public async Task Api_Me_Detail_TwoFactor_Success()
        {
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var status = await UoW.UserMgmt.SetTwoFactorEnabledAsync(user.Id, false);
            status.Should().BeAssignableTo(typeof(IdentityResult));
            status.Succeeded.Should().BeTrue();

            var result = await controller.SetTwoFactor(true) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_Update_Success()
        {
            var controller = new DetailController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)"
            };

            var result = await controller.UpdateDetail(model) as OkNegotiatedContentResult<UserModel>;
            result.Content.Should().BeAssignableTo(typeof(UserModel));
            result.Content.FirstName.ShouldBeEquivalentTo(model.FirstName);
            result.Content.LastName.ShouldBeEquivalentTo(model.LastName);
        }
    }
}
