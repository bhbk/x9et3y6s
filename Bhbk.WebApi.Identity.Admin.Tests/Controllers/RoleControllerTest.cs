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
    public class RoleControllerTest : StartupTest
    {
        private TestServer _owin;

        public RoleControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_AddToUser_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = _ioc.AudienceMgmt.Store.Get().First().Id,
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
            };
            var create = await _ioc.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await _ioc.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            controller.SetUser(user.Id);

            var result = await controller.AddRoleToUserV1(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Delete_Fail_Immutable()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.RoleMgmt.Store.SetImmutableAsync(role, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _ioc.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Create_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var model = new RoleCreate()
            {
                AudienceId = _ioc.AudienceMgmt.Store.Get().First().Id,
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
                Immutable = false
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateRoleV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _ioc.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetById_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.GetRoleV1(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetByName_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.GetRoleV1(role.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateDefault();
            _data.CreateRandom(10);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomNumber.CreateBase64(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "name";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/role/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Fail_ParamInvalid()
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

            string order = "name";

            var response = await request.GetAsync("/role/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Success()
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

            string order = "name";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/role/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetUserList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await controller.GetRoleUsersV1(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(_ioc.RoleMgmt.Store.GetUsersAsync(role).Count());
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_RemoveFromUser_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = _ioc.AudienceMgmt.Store.Get().First().Id,
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
                Immutable = false
            };

            var create = await _ioc.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await _ioc.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var add = await _ioc.UserMgmt.AddToRoleAsync(user, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            controller.SetUser(user.Id);

            var result = await controller.RemoveRoleFromUserV1(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Update_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new RoleController(_conf, _ioc, _tasks);
            var role = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var model = new RoleUpdate()
            {
                Id = role.Id,
                AudienceId = _ioc.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "(Updated)",
                Enabled = true,
                Immutable = false
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.UpdateRoleV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
