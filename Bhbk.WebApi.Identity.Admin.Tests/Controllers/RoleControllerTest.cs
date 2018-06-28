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
        public async Task Api_Admin_Role_AddToUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = TestIoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
            };
            var create = await TestIoC.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await TestIoC.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            TestController.SetUser(user.Id);

            var result = await TestController.AddRoleToUser(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Fail_Immutable()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var role = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.RoleMgmt.Store.SetImmutableAsync(role, true);
            TestController.SetUser(user.Id);

            var result = await TestController.DeleteRole(role.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = TestIoC.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var model = new RoleCreate()
            {
                AudienceId = TestIoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
                Immutable = false
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateRole(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Role_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var role = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.DeleteRole(role.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.RoleMgmt.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Role_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var role = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await TestController.GetRole(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Role_GetList_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new RoleController(TestIoC, TestTasks);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(32));
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
        public async Task Api_Admin_Role_GetList_Fail_ParamInvalid()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new RoleController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.GenerateAccessTokenV2(TestIoC, client, audiences, user).Result;

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
        public async Task Api_Admin_Role_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var TestController = new RoleController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtHelper.GenerateAccessTokenV2(TestIoC, client, audiences, user).Result;

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
        public async Task Api_Admin_Role_GetUserList_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var role = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();

            var result = await TestController.GetRoleUsers(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(TestIoC.RoleMgmt.Store.GetUsersAsync(role).Count());
        }

        [TestMethod]
        public async Task Api_Admin_Role_RemoveFromUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new RoleCreate()
            {
                AudienceId = TestIoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestRoleA,
                Enabled = true,
                Immutable = false
            };

            var create = await TestIoC.RoleMgmt.CreateAsync(new RoleFactory<AppRole>(model).Devolve());
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            var role = await TestIoC.RoleMgmt.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var add = await TestIoC.UserMgmt.AddToRoleAsync(user, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            TestController.SetUser(user.Id);

            var result = await TestController.RemoveRoleFromUser(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Role_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var TestController = new RoleController(TestIoC, TestTasks);

            var role = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleA).Single();
            var model = new RoleUpdate()
            {
                Id = role.Id,
                AudienceId = TestIoC.AudienceMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestRoleA + "(Updated)",
                Enabled = true,
                Immutable = false
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.UpdateRole(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
