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
        public async Task Api_Admin_LoginV1_AddToUser_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var login = new LoginCreate()
            {
                LoginProvider = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
            };
            var add = await _ioc.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKeyA,
                Enabled = true,
            };

            controller.SetUser(user.Id);

            var result = await controller.AddLoginToUserV1(model.LoginId, model.UserId, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Delete_Fail_Immutable()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.LoginMgmt.Store.SetImmutableAsync(login, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteLoginV1(login.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _ioc.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Create_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new LoginCreate()
            {
                LoginProvider = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
            };

            controller.SetUser(user.Id);

            var result = await controller.CreateLoginV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_RemoveFromUser_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var login = new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Helpers.CryptoHelper.CreateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
                }).Devolve();
            var create = await _ioc.LoginMgmt.CreateAsync(login);
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

            controller.SetUser(user.Id);

            var add = await _ioc.UserMgmt.AddLoginAsync(user, 
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUserV1(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetById_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.GetLoginV1(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetByName_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.GetLoginV1(login.LoginProvider) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetList_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateRandom(10);

            var controller = new LoginController(_conf, _ioc, _tasks);
            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32));
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
        public async Task Api_Admin_LoginV1_GetList_Fail_ParamInvalid()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateRandom(10);

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.CreateAccessTokenV2(_ioc, client, audiences, user).Result;

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
        public async Task Api_Admin_LoginV1_GetList_Success()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateRandom(10);

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.CreateAccessTokenV2(_ioc, client, audiences, user).Result;

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
        public async Task Api_Admin_LoginV1_GetUserList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.GetLoginUsersV1(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(_ioc.LoginMgmt.Store.GetUsers(login.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Update_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var model = new LoginUpdate()
            {
                Id = login.Id,
                LoginProvider = login.LoginProvider + "(Updated)"
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.UpdateLoginV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new LoginController(_conf, _ioc, _tasks);
            var login = _ioc.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteLoginV1(login.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _ioc.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeFalse();
        }
    }
}
