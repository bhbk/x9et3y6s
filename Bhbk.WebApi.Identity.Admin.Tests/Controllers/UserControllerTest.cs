using Bhbk.Lib.Helpers.Cryptography;
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
        public async Task Api_Admin_UserV1_AddPassword_Fail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await _ioc.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = RandomNumber.CreateBase64(16),
                NewPasswordConfirm = RandomNumber.CreateBase64(16)
            };

            controller.SetUser(user.Id);

            var result = await controller.AddPasswordV1(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _ioc.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_AddPassword_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await _ioc.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestUserPassNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestUserPassNew
            };

            controller.SetUser(user.Id);

            var result = await controller.AddPasswordV1(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _ioc.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Create_Fail_InvalidEmail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var model = new UserCreate()
            {
                Email = BaseLib.Statics.ApiUnitTestUserA + "-" + RandomNumber.CreateBase64(4),
                FirstName = RandomNumber.CreateBase64(4) + "-First",
                LastName = RandomNumber.CreateBase64(4) + "-Last",
                PhoneNumber = RandomNumber.CreateNumberAsString(10),
                LockoutEnabled = false,
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateUserV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Create_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var model = new UserCreate()
            {
                ClientId = client.Id,
                Email = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestUserA,
                FirstName = RandomNumber.CreateBase64(4) + "-First",
                LastName = RandomNumber.CreateBase64(4) + "-Last",
                PhoneNumber = RandomNumber.CreateNumberAsString(10),
                LockoutEnabled = false,
                Immutable = false,
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateUserV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Delete_Fail_Immutable()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.UserMgmt.Store.SetImmutableAsync(user, true).Wait();
            controller.SetUser(user.Id);

            var result = await controller.DeleteUserV1(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _ioc.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteUserV1(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _ioc.UserMgmt.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Get_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetList_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateRandom(10);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomNumber.CreateBase64(32));
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
        public async Task Api_Admin_UserV1_GetList_Fail_ParamInvalid()
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

            string order = "email";

            var response = await request.GetAsync("/user/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetList_Success()
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
        public async Task Api_Admin_UserV1_GetByName_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserV1(user.UserName) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetAudienceList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserAudiencesV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be((await _ioc.UserMgmt.Store.GetAudiencesAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetLoginList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserLoginsV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

            data.Count().Should().Be((await _ioc.UserMgmt.Store.GetLoginsAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetRoleList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await controller.GetUserRolesV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be((await _ioc.UserMgmt.Store.GetRolesResultIdAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_RemovePassword_Fail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var remove = await _ioc.UserMgmt.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            controller.SetUser(user.Id);

            var result = await controller.RemovePasswordV1(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _ioc.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_RemovePassword_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.RemovePasswordV1(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _ioc.UserMgmt.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_SetPassword_Fail()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = RandomNumber.CreateBase64(16),
                NewPasswordConfirm = RandomNumber.CreateBase64(16)
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _ioc.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_SetPassword_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserAddPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestUserPassNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestUserPassNew
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _ioc.UserMgmt.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Update_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new UserController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new UserUpdate()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                LockoutEnabled = false,
                LockoutEnd = DateTime.Now.AddDays(30)
            };

            controller.SetUser(user.Id);

            var result = await controller.UpdateUserV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
