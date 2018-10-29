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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new RoleCreate()
            {
                ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestRole1,
                Enabled = true,
            };

            var create = await _uow.CustomRoleMgr.CreateAsync(
                _uow.Convert.Map<AppRole>(
                    new RoleCreate()
                    {
                        ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                        Name = model.Name,
                        Enabled = true,
                    }));
            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            await _uow.CommitAsync();

            var role = await _uow.CustomRoleMgr.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var result = await controller.AddRoleToUserV1(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.CustomRoleMgr.Store.SetImmutableAsync(role, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _uow.CustomRoleMgr.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Create_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var model = new RoleCreate()
            {
                ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestRole1,
                Enabled = true,
                Immutable = false
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateRoleV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _uow.CustomRoleMgr.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetById_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();

            var result = await controller.GetRoleV1(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();

            var result = await controller.GetRoleV1(role.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Fail_Auth()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomValues.CreateBase64String(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "name";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/role/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Fail_ParamInvalid()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var clients = new List<AppClient>();
            clients.Add(client);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, clients, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "name";

            var response = await request.GetAsync("/role/v1?"
                + "orderBy=" + orderBy);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetList_Success()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var clients = new List<AppClient>();
            clients.Add(client);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, clients, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "name";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/role/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<RoleResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be(take);
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_GetUserList_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();

            var result = await controller.GetRoleUsersV1(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(_uow.CustomRoleMgr.Store.GetUsersAsync(role).Count());
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_RemoveFromUser_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new RoleCreate()
            {
                ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestRole1,
                Enabled = true,
                Immutable = false
            };

            var create = await _uow.CustomRoleMgr.CreateAsync(
                _uow.Convert.Map<AppRole>(
                    new RoleCreate()
                    {
                        ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                        Name = model.Name,
                        Enabled = true,
                        Immutable = false
                    }));

            create.Should().BeAssignableTo(typeof(IdentityResult));
            create.Succeeded.Should().BeTrue();

            await _uow.CommitAsync();

            var role = await _uow.CustomRoleMgr.FindByNameAsync(model.Name);
            role.Should().BeAssignableTo(typeof(AppRole));

            var add = await _uow.CustomUserMgr.AddToRoleAsync(user, model.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            await _uow.CommitAsync();

            var result = await controller.RemoveRoleFromUserV1(role.Id, user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_RoleV1_Update_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new RoleController(_conf, _uow, _tasks);
            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new RoleUpdate()
            {
                Id = role.Id,
                ClientId = (await _uow.ClientRepo.GetAsync()).First().Id,
                Name = Strings.ApiUnitTestRole1 + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateRoleV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
