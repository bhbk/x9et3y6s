using Bhbk.Lib.Identity.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        private S2STester _s2s;

        public OAuthControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STester(_conf, _ioc.ContextStatus, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_GetList_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.GetAsync("/oauth/v1/refresh/" + user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_GetList_Success()
        {
            _data.Destroy();
            _data.CreateTest();
            _data.CreateDefault();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.GetAsync("/oauth/v1/refresh/" + user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_RevokeAll_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_RevokeAll_Success()
        {
            _data.Destroy();
            _data.CreateTest();
            _data.CreateDefault();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_RevokeOne_Fail_Auth()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke/" + refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_Refresh_RevokeOne_Success()
        {
            _data.Destroy();
            _data.CreateTest();
            _data.CreateDefault();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearer);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await request.DeleteAsync("/oauth/v1/refresh/" + user.Id.ToString() + "/revoke/" + refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV1_RequestCode_Fail_NotImplemented()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);

            var result = await _s2s.StsAuthorizationCodeRequestV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV2_RequestCode_Fail_AudienceInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);

            var result = await _s2s.StsAuthorizationCodeRequestV2(client.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), redirect.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV2_RequestCode_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);

            var result = await _s2s.StsAuthorizationCodeRequestV2(Guid.NewGuid().ToString(), audience.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV2_RequestCode_Fail_UriInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = new Uri("https://app.test.net/a/invalid");

            var result = await _s2s.StsAuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV2_RequestCode_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = Guid.NewGuid();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);

            var result = await _s2s.StsAuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.ToString(), redirect.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuthV2_RequestCode_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);

            var result = await _s2s.StsAuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            //not done yet...
            var pairs = result.Headers.GetValues("Set-Cookie");

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            HttpClient http = new HttpClient(handler);
            HttpResponseMessage response = result;

            Uri uri = new Uri("http://google.com");
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
                Console.WriteLine(cookie.Name + ": " + cookie.Value);

            var check = new Uri(await result.Content.ReadAsStringAsync());
            check.Should().BeAssignableTo(typeof(Uri));
        }
    }
}
