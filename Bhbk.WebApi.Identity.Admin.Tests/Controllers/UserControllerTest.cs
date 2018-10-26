using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
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
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var remove = await _uow.CustomUserMgr.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = RandomValues.CreateBase64String(16),
                NewPasswordConfirm = RandomValues.CreateBase64String(16)
            };

            controller.SetUser(user.Id);

            var result = await controller.AddPasswordV1(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _uow.CustomUserMgr.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_AddPassword_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var remove = await _uow.CustomUserMgr.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            var model = new UserAddPassword()
            {
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            controller.SetUser(user.Id);

            var result = await controller.AddPasswordV1(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _uow.CustomUserMgr.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Create_Fail_InvalidEmail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var model = new UserCreate()
            {
                ClientId = client.Id,
                Email = Strings.ApiUnitTestUser1 + "?" + RandomValues.CreateBase64String(4),
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateUserV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Create_Success_Standard()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var model = new UserCreate()
            {
                ClientId = client.Id,
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateUserV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Create_Success_System()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var model = new UserCreate()
            {
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = false,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateUserV1NoConfirm(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.CustomUserMgr.Store.SetImmutableAsync(user, true).Wait();
            controller.SetUser(user.Id);

            var result = await controller.DeleteUserV1(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _uow.CustomUserMgr.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteUserV1(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _uow.CustomUserMgr.Store.Get(x => x.Id == user.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Get_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetUserV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetList_Fail_Auth()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomValues.CreateBase64String(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "email";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/user/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetList_Fail_ParamInvalid()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "email";

            var response = await request.GetAsync("/user/v1?"
                + "orderBy=" + orderBy);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetList_Success()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "email";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/user/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<UserResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(take);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetUserV1(user.UserName) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetAudienceList_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetUserAudiencesV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be((await _uow.CustomUserMgr.Store.GetAudiencesAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetLoginList_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetUserLoginsV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

            data.Count().Should().Be((await _uow.CustomUserMgr.Store.GetLoginsAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_GetRoleList_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await controller.GetUserRolesV1(user.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be((await _uow.CustomUserMgr.Store.GetRolesResultIdAsync(user)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_RemovePassword_Fail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var remove = await _uow.CustomUserMgr.RemovePasswordAsync(user);
            remove.Should().BeAssignableTo(typeof(IdentityResult));
            remove.Succeeded.Should().BeTrue();

            controller.SetUser(user.Id);

            var result = await controller.RemovePasswordV1(user.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _uow.CustomUserMgr.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_RemovePassword_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.RemovePasswordV1(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _uow.CustomUserMgr.HasPasswordAsync(user);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_SetPassword_Fail()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var model = new UserAddPassword()
            {
                NewPassword = RandomValues.CreateBase64String(16),
                NewPasswordConfirm = RandomValues.CreateBase64String(16)
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(user.Id, model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = await _uow.CustomUserMgr.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_SetPassword_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var model = new UserAddPassword()
            {
                NewPassword = Strings.ApiUnitTestUserPassNew,
                NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
            };

            controller.SetUser(user.Id);

            var result = await controller.SetPasswordV1(user.Id, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await _uow.CustomUserMgr.CheckPasswordAsync(user, model.NewPassword);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_UserV1_Update_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new UserController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new UserUpdate()
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)",
                LockoutEnabled = false,
                LockoutEnd = DateTime.Now.AddDays(30),
                HumanBeing = false,
                Immutable = false,
            };

            var result = await controller.UpdateUserV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<UserResult>().Subject;

            data.FirstName.Should().Be(model.FirstName);
            data.LastName.Should().Be(model.LastName);
        }
    }
}
