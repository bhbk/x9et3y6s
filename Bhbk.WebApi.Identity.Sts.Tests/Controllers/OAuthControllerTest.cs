using Bhbk.Lib.Identity.Interop;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        private StsTester _sts;

        public OAuthControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _sts = new StsTester(_conf, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_GetList_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_GetList_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_RevokeAll_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenDeleteAllV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_RevokeAll_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteAllV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_RevokeOne_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV1(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_RevokeOne_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV1(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_GetList_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_GetList_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_RevokeAll_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenDeleteAllV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_RevokeAll_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteAllV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_RevokeOne_Fail_Auth()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV2(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_RevokeOne_Success()
        {
            _tests.DestroyAll();
            _tests.Create();
            _defaults.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiDefaultClient).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV2(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RequestCodeV1_Fail_NotImplemented()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RequestCodeV2_Fail_AudienceNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(client.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RequestCodeV2_Fail_ClientNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(Guid.NewGuid().ToString(), audience.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RequestCodeV2_Fail_UriNotValid()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var url = new Uri("https://app.test.net/a/invalid");

            var result = await _sts.AuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RequestCodeV2_Fail_UserNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = Guid.NewGuid();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, Ignore]
        public async Task Api_Sts_OAuth_RequestCodeV2_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);

            ////not done yet...
            //var pairs = result.Headers.GetValues("Set-Cookie");

            //CookieContainer cookies = new CookieContainer();
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.CookieContainer = cookies;

            //HttpClient http = new HttpClient(handler);
            //HttpResponseMessage response = result;

            //Uri uri = new Uri("http://google.com");
            //IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            //foreach (Cookie cookie in responseCookies)
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);
        }
    }
}
