using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
    public class ActivityControllerTest : StartupTest
    {
        private TestServer _owin;

        public ActivityControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_ActivityV1_GetList_Fail_Auth()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomValues.CreateBase64String(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "created";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/activity/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_ActivityV1_GetList_Fail_ParamInvalid()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, client, new List<AppAudience> { audience }, user);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "created";

            var response = await request.GetAsync("/activity/v1?"
                + "orderBy=" + orderBy);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_ActivityV1_GetList_Success()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClient)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiDefaultAudienceUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_uow, client, new List<AppAudience> { audience }, user);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "created";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/activity/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ActivityResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<ActivityResult>>().Subject;

            data.Count().Should().Be(take);
        }
    }
}
