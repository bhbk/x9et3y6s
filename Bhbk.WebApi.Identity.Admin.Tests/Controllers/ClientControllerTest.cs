using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
    public class ClientControllerTest : StartupTest
    {
        private TestServer _owin;

        public ClientControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Create_Fail_ClientType()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var model = new ClientCreate()
            {
                IssuerId = (await _uow.IssuerRepo.GetAsync()).First().Id,
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestClient1,
                ClientType = RandomValues.CreateBase64String(8),
                Enabled = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateClientV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            client.Immutable = true;
            await _uow.ClientRepo.UpdateAsync(client);

            controller.SetUser(user.Id);

            var result = await controller.DeleteClientV1(client.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = (await _uow.ClientRepo.GetAsync(x => x.Id == client.Id)).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Create_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var model = new ClientCreate()
            {
                IssuerId = (await _uow.IssuerRepo.GetAsync()).First().Id,
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestClient1,
                ClientType = Enums.ClientType.user_agent.ToString(),
                Enabled = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateClientV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteClientV1(client.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = (await _uow.ClientRepo.GetAsync(x => x.Id == client.Id)).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetById_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.GetClientV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.GetClientV1(client.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Fail_Auth()
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

            var response = await request.GetAsync("/client/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Fail_ParamInvalid()
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

            string order = "name";

            var response = await request.GetAsync("/client/v1?"
                + "orderBy=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Success()
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

            string order = "desc";
            string orderBy = "name";
            ushort skip = 1;
            ushort take = 3;

            var response = await request.GetAsync("/client/v1?"
                + "filter=" + string.Empty + "&"
                + "order=" + order + "&"
                + "orderBy=" + orderBy + "&"
                + "skip=" + skip.ToString() + "&"
                + "take=" + take.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["items"].ToString()).ToObject<IEnumerable<ClientResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<ClientResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _uow.ClientRepo.Count());
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetRoleList_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await controller.GetClientRolesV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be((await _uow.ClientRepo.GetRoleListAsync(client.Id)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Update_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new ClientUpdate()
            {
                IssuerId = (await _uow.IssuerRepo.GetAsync()).First().Id,
                ActorId = user.Id,
                Id = client.Id,
                Name = Strings.ApiUnitTestClient1 + "(Updated)",
                Description = client.Description ?? string.Empty,
                ClientType = client.ClientType,
                Enabled = true,
                Immutable = client.Immutable,
            };

            var result = await controller.UpdateClientV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
