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
    public class IssuerControllerTest : StartupTest
    {
        private TestServer _owin;

        public IssuerControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_Create_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var model = new IssuerCreate()
            {
                Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestIssuer1,
                IssuerKey = RandomValues.CreateBase64String(32),
                Enabled = true,
            };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateIssuerV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            issuer.Immutable = true;
            await _uow.IssuerRepo.UpdateAsync(issuer);
            await _uow.CommitAsync();
        
            controller.SetUser(user.Id);

            var result = await controller.DeleteIssuerV1(issuer.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = (await _uow.IssuerRepo.GetAsync(x => x.Id == issuer.Id)).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteIssuerV1(issuer.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = (await _uow.IssuerRepo.GetAsync(x => x.Id == issuer.Id)).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetById_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();

            var result = await controller.GetIssuerV1(issuer.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Id.Should().Be(issuer.Id);
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();

            var result = await controller.GetIssuerV1(issuer.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Id.Should().Be(issuer.Id);
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetList_Fail_Auth()
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

            var response = await request.GetAsync("/issuer/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetList_Fail_ParamInvalid()
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

            var response = await request.GetAsync("/issuer/v1?"
                + "orderBy=" + orderBy);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetList_Success()
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

            string order = "asc";
            string orderBy = "name";
            ushort skip = 1;
            ushort take = 3;

            var response = await request.GetAsync("/issuer/v1?"
                + "filter=" + string.Empty + "&"
                + "order=" + order + "&"
                + "orderBy=" + orderBy + "&"
                + "skip=" + skip.ToString() + "&"
                + "take=" + take.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["items"].ToString()).ToObject<IEnumerable<IssuerResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<IssuerResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _uow.IssuerRepo.Count());
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_GetClientList_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();

            var result = await controller.GetIssuerClientsV1(issuer.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<ClientResult>>().Subject;

            data.Count().Should().Be((await _uow.IssuerRepo.GetClientsAsync(issuer.Id)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_IssuerV1_Update_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new IssuerController(_conf, _uow, _tasks);
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new IssuerUpdate()
            {
                Id = issuer.Id,
                Name = Strings.ApiUnitTestIssuer1 + "(Updated)",
                IssuerKey = issuer.IssuerKey,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateIssuerV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IssuerResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
