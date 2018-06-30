using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
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
using BaseLib = Bhbk.Lib.Identity;

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
        public async Task Api_Admin_Activity_GetList_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", BaseLib.Helpers.CryptoHelper.CreateRandomBase64(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "created";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/activity/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_Activity_GetList_Fail_ParamInvalid()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var access = await JwtHelper.GetAccessTokenV2(TestIoC,
                BaseLib.Statics.ApiDefaultClient, BaseLib.Statics.ApiDefaultAudienceUi, BaseLib.Statics.ApiDefaultUserAdmin);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "created";

            var response = await request.GetAsync("/activity/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_Activity_GetList_Pass()
        {
            TestData.Destroy();
            TestData.CreateDefault();
            TestData.CreateRandom(10);

            var access = await JwtHelper.GetAccessTokenV2(TestIoC,
                BaseLib.Statics.ApiDefaultClient, BaseLib.Statics.ApiDefaultAudienceUi, BaseLib.Statics.ApiDefaultUserAdmin);

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "created";
            ushort size = 3;
            ushort page = 1;

            var response = await request.GetAsync("/activity/v1?"
                + BaseLib.Statics.GetOrderBy + "=" + order + "&"
                + BaseLib.Statics.GetPageSize + "=" + size.ToString() + "&"
                + BaseLib.Statics.GetPageNumber + "=" + page.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<ActivityResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<ActivityResult>>().Subject;

            data.Count().Should().Be(size);
        }
    }
}
