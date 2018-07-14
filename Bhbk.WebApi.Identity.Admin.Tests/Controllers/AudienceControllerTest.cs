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
    public class AudienceControllerTest : StartupTest
    {
        private TestServer _owin;

        public AudienceControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_Create_Fail_AudienceType()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var model = new AudienceCreate()
            {
                ClientId = _ioc.ClientMgmt.Store.Get().First().Id,
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestAudienceA,
                AudienceType = RandomNumber.CreateBase64(8),
                Enabled = true,
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateAudienceV1(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_Delete_Fail_Immutable()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.AudienceMgmt.Store.SetImmutableAsync(audience, true);
            controller.SetUser(user.Id);

            var result = await controller.DeleteAudienceV1(audience.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = _ioc.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_Create_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var model = new AudienceCreate()
            {
                ClientId = _ioc.ClientMgmt.Store.Get().First().Id,
                Name = RandomNumber.CreateBase64(4) + "-" + BaseLib.Statics.ApiUnitTestAudienceA,
                AudienceType = BaseLib.AudienceType.user_agent.ToString(),
                Enabled = true,
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.CreateAudienceV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_Delete_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteAudienceV1(audience.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = _ioc.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetById_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.GetAudienceV1(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetByName_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.GetAudienceV1(audience.Name) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetList_Fail_Auth()
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

            var response = await request.GetAsync("/audience/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetList_Fail_ParamInvalid()
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

            var response = await request.GetAsync("/audience/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetList_Success()
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

            var response = await request.GetAsync("/audience/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<AudienceResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<AudienceResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_GetRoleList_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await controller.GetAudienceRolesV1(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

            data.Count().Should().Be(_ioc.AudienceMgmt.Store.GetRoles(audience.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_AudienceV1_Update_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var controller = new AudienceController(_conf, _ioc, _tasks);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var model = new AudienceUpdate()
            {
                Id = audience.Id,
                ClientId = _ioc.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestAudienceA + "(Updated)",
                AudienceType = audience.AudienceType,
                Enabled = true
            };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            controller.SetUser(user.Id);

            var result = await controller.UpdateAudienceV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
