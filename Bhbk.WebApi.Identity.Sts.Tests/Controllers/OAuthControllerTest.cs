using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public async Task Api_OAuth_RefreshToken_Revoke_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV1.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var token = user.AppUserRefresh.Where(x => x.ProtectedTicket == refresh).Single();

            var result = await controller.RevokeRefreshToken(user.Id, token.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshToken_Revoke_Success_ViaEndPoint()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV1.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var bearer = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];
            var token = user.AppUserRefresh.Where(x => x.ProtectedTicket == refresh).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearer);

            //var result = await controller.RevokeRefreshToken(user.Id, token.Id) as NoContentResult;
            var result = request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke/" + token.Id.ToString()).Result;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_OAuth_RefreshTokens_Revoke_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new OAuthController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV1.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await controller.RevokeRefreshTokens(user.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
