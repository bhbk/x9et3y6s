using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
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
using BaseLib = Bhbk.Lib.Identity;

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
        public async Task Api_Me_Detail_AskChangeEmail_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string email = "unit-test@" + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4),
                NewEmail = email,
                NewEmailConfirm = email
            };

            var result = await controller.AskChangeEmail(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            //var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var result = await _owin.HttpClient.PutAsync("/detail/v1/" + user.Id.ToString() + "/change-password", content);
            //result.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangeEmail_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string email = "unit-test@" + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangeEmail()
            {
                Id = user.Id,
                CurrentEmail = user.Email,
                NewEmail = email,
                NewEmailConfirm = email
            };

            var result = await controller.AskChangeEmail(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;

            //var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            //var result = await _owin.HttpClient.PutAsync("/detail/v1/" + user.Id.ToString() + "/change-password", content);
            //result.Content.Should().BeAssignableTo(typeof(string));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePassword_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AskChangePassword(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePassword_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePassword()
            {
                Id = user.Id,
                CurrentPassword = BaseLib.Statics.ApiUnitTestPasswordCurrent,
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            var result = await controller.AskChangePassword(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePhone_Fail()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string phone = "01112223333";
            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = phone,
                NewPhoneNumber = user.PhoneNumber,
                NewPhoneNumberConfirm = user.PhoneNumber
            };

            var result = await controller.AskChangePhone(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_AskChangePhone_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string phone = "01112223333";
            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserChangePhone()
            {
                Id = user.Id,
                CurrentPhoneNumber = user.PhoneNumber,
                NewPhoneNumber = phone,
                NewPhoneNumberConfirm = phone
            };

            var result = await controller.AskChangePhone(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<string>().Subject;
        }

        [TestMethod]
        public void Api_Me_Detail_GetClaimList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var result = controller.GetClaims() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<Claim>>().Subject;
        }

        [TestMethod]
        public async Task Api_Me_Detail_TwoFactor_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var status = await Context.UserMgmt.SetTwoFactorEnabledAsync(user, false);
            status.Should().BeAssignableTo(typeof(IdentityResult));
            status.Succeeded.Should().BeTrue();

            var result = await controller.SetTwoFactor(true) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Me_Detail_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new DetailController(Context);
            var user = Context.UserMgmt.Store.Get().First();

            controller.SetUser(user.Id);

            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)"
            };

            var result = await controller.UpdateDetail(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
