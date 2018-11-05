using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class RoleControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _sp;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly AdminClient _admin;

        public RoleControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _sp = fake.Server.Host.Services;
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _admin = new AdminClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Admin_RoleV1_AddToUser_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Admin_RoleV1_Delete_Fail()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.CustomRoleMgr.Store.SetImmutableAsync(role, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _uow.CustomRoleMgr.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Admin_RoleV1_Delete_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteRoleV1(role.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _uow.CustomRoleMgr.Store.Get(x => x.Id == role.Id).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_RoleV1_Get_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();

            var result = await controller.GetRoleV1(role.Id.ToString()) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);

            result = await controller.GetRoleV1(role.Name) as OkObjectResult;
            ok = result.Should().BeOfType<OkObjectResult>().Subject;
            data = ok.Value.Should().BeAssignableTo<RoleResult>().Subject;

            data.Id.Should().Be(role.Id);
        }

        [Fact]
        public async Task Admin_RoleV1_GetList_Fail()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).CreateRandom(10);
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("name", "asc"));

            var pager = new TuplePager()
            {
                Filter = string.Empty,
                Orders = orders,
            };

            var response = await _admin.RoleGetPagesV1(RandomValues.CreateBase64String(32), pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, new List<AppClient> { client }, user);

            response = await _admin.RoleGetPagesV1(access.token, pager);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_RoleV1_GetList_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).CreateRandom(10);
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, new List<AppClient> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("name", "asc"));

            var response = await _admin.RoleGetPagesV1(access.token,
                new TuplePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<RoleResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<RoleResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _uow.CustomRoleMgr.Store.Count());
        }

        [Fact]
        public async Task Admin_RoleV1_GetListUsers_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var role = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole1).Single();

            var result = await controller.GetRoleUsersV1(role.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be(_uow.CustomRoleMgr.Store.GetUsersAsync(role).Count());
        }

        [Fact]
        public async Task Admin_RoleV1_RemoveFromUser_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Admin_RoleV1_Update_Success()
        {
            var controller = new RoleController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _sp;

            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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
