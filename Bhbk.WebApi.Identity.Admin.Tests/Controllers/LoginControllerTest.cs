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
    public class LoginControllerTest : StartupTest
    {
        private TestServer _owin;

        public LoginControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Login_AddToUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var login = new LoginCreate()
            {
                LoginProvider = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
            };
            var add = await TestIoC.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKeyA,
                Enabled = true,
            };

            TestController.SetUser(user.Id);

            var result = await TestController.AddLoginToUser(model.LoginId, model.UserId, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Delete_Fail_Immutable()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var login = TestIoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.LoginMgmt.Store.SetImmutableAsync(login, true);
            TestController.SetUser(user.Id);

            var result = await TestController.DeleteLogin(login.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = TestIoC.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_Login_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new LoginCreate()
            {
                LoginProvider = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
            };

            TestController.SetUser(user.Id);

            var result = await TestController.CreateLogin(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_RemoveFromUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var login = new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
                }).Devolve();
            var create = await TestIoC.LoginMgmt.CreateAsync(login);
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = create.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKeyA,
                Enabled = true,
                Immutable = false
            };

            TestController.SetUser(user.Id);

            var add = await TestIoC.UserMgmt.AddLoginAsync(user, 
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await TestController.RemoveLoginFromUser(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var login = TestIoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await TestController.GetLogin(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetList_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new LoginController(TestIoC, TestTasks);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "loginprovider";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/login/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetList_Fail_ParamInvalid()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new LoginController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.GenerateAccessTokenV2(TestIoC, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "loginprovider";

            var response = await request.GetAsync("/login/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new LoginController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.GenerateAccessTokenV2(TestIoC, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "loginprovider";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/login/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetUserList_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var login = TestIoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await TestController.GetLoginUsers(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(TestIoC.LoginMgmt.Store.GetUsers(login.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_Login_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var login = TestIoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var model = new LoginUpdate()
            {
                Id = login.Id,
                LoginProvider = login.LoginProvider + "(Updated)"
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.UpdateLogin(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new LoginController(TestIoC, TestTasks);

            var login = TestIoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.DeleteLogin(login.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeFalse();
        }
    }
}
