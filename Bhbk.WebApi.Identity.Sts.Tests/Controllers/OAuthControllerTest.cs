using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class OAuthControllerTest : StartupTest
    {
        private TestServer _owin;

        public OAuthControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshToken_GetList_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.GetAsync("/oauth/v1/refresh/" + user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshToken_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateDefaultData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.GetAsync("/oauth/v1/refresh/" + user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshToken_Revoke_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke/" + refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshToken_Revoke_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateDefaultData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke/" + refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshTokens_Revoke_Fail_Auth()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshTokens_Revoke_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateDefaultData();

            var controller = new OAuthController(TestIoC, TestTasks);
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultAudienceUi).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
