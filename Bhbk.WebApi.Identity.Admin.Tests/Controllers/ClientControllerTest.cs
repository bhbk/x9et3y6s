using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models;
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
using BaseLib = Bhbk.Lib.Identity;
using Bhbk.Lib.Helpers.Cryptography;

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
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var model = new ClientCreate()
            {
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestClientA,
                ClientKey = RandomNumber.CreateBase64(32),
                Enabled = true,
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateClientV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Delete_Fail_Immutable()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.ClientMgmt.Store.SetImmutableAsync(client, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteClientV1(client.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _ioc.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteClientV1(client.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _ioc.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetById_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await controller.GetClientV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetByName_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await controller.GetClientV1(client.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Fail_Auth()
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

            var response = await request.GetAsync("/client/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Fail_ParamInvalid()
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

            var response = await request.GetAsync("/client/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetList_Success()
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

            var response = await request.GetAsync("/client/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ClientResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<ClientResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_GetAudienceList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await controller.GetClientAudiencesV1(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be(_ioc.ClientMgmt.Store.GetAudiences(client.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_ClientV1_Update_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new ClientController(_conf, _ioc, _tasks);
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = BaseLib.Statics.ApiUnitTestClientA + "(Updated)",
                ClientKey = client.ClientKey,
                Enabled = true,
                Immutable = false
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.UpdateClientV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
