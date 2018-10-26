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
        public async Task Api_Admin_ClientV1_Create_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var model = new ClientCreate()
            {
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestClient1,
                ClientKey = RandomValues.CreateBase64String(32),
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
        public async Task Api_Admin_ClientV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            client.Immutable = true;
            await _uow.ClientRepo.UpdateAsync(client);
            await _uow.CommitAsync();
        
            controller.SetUser(user.Id);

            var result = await controller.DeleteClientV1(client.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = (await _uow.ClientRepo.GetAsync(x => x.Id == client.Id)).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

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
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await controller.GetClientV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

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

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var audiences = new List<AppAudience>();
            audiences.Add(audience);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, client, audiences, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "name";

            var response = await request.GetAsync("/client/v1?"
                + "orderBy=" + orderBy);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Success()
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

            string orderBy = "name";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/client/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<ClientResult>>().Subject;

            data.Count().Should().Be(take);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetAudienceList_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();

            var result = await controller.GetClientAudiencesV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be((await _uow.ClientRepo.GetAudiencesAsync(client.Id)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Update_Success()
        {
            _tests.Destroy();
            _tests.TestsCreate();

            var controller = new ClientController(_conf, _uow, _tasks);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = Strings.ApiUnitTestClient1 + "(Updated)",
                ClientKey = client.ClientKey,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateClientV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
