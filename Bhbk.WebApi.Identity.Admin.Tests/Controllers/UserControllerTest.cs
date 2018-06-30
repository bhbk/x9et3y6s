using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class UserControllerTest : StartupTest
    {
        private TestServer _owin;

        public UserControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Fail_InvalidEmail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserA + "-" + BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4),
                FirstName = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-First",
                LastName = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-Last",
                PhoneNumber = "0123456789",
                LockoutEnabled = false,
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateUser(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var model = new UserCreate()
            {
                Email = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestUserA,
                FirstName = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-First",
                LastName = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-Last",
                PhoneNumber = "0123456789",
                LockoutEnabled = false,
                Immutable = false,
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateUser(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_User_Delete_Fail_Immutable()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.UserMgmt.Store.SetImmutableAsync(user, true).Wait();
            TestController.SetUser(user.Id);

            var result = await TestController.DeleteUser(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = TestIoC.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_Delete_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.DeleteUser(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_Get_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestController.GetUser(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new UserController(TestIoC, TestTasks);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "email";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/user/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Fail_ParamInvalid()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new UserController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.CreateAccessTokenV2(TestIoC, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "email";

            var response = await request.GetAsync("/user/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetList_Pass()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new UserController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.CreateAccessTokenV2(TestIoC, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "email";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/user/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<UserResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetByName_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestController.GetUser(user.UserName) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetAudienceList_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestController.GetUserAudiences(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be((await TestIoC.UserMgmt.Store.GetAudiencesAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetLoginList_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestController.GetUserLogins(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

            data.Count().Should().Be((await TestIoC.UserMgmt.Store.GetLoginsAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetRoleList_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestController.GetUserRoles(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be((await TestIoC.UserMgmt.Store.GetRolesReturnIdAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await TestIoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(16),
                NewPasswordConfirm = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(16)
            };

            TestController.SetUser(user.Id);

            var result = await TestController.AddPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await TestIoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_AddPassword_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await TestIoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            TestController.SetUser(user.Id);

            var result = await TestController.AddPassword(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await TestIoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await TestIoC.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            TestController.SetUser(user.Id);

            var result = await TestController.RemovePassword(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await TestIoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_RemovePassword_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.RemovePassword(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await TestIoC.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Fail()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(16),
                NewPasswordConfirm = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(16)
            };

            TestController.SetUser(user.Id);

            var result = await TestController.ResetPassword(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await TestIoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_User_ResetPassword_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestPasswordNew
            };

            TestController.SetUser(user.Id);

            var result = await TestController.ResetPassword(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await TestIoC.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_User_Update_Pass()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new UserController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                LockoutEnabled = false,
                LockoutEnd = DateTime.Now.AddDays(30)
            };

            TestController.SetUser(user.Id);

            var result = await TestController.UpdateUser(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
