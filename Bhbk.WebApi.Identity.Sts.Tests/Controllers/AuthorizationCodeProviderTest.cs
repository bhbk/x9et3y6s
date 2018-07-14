using Bhbk.Lib.Helpers.Cryptography;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class AuthorizationCodeProviderTest : StartupTest
    {
        private TestServer _owin;
        private S2STests _s2s;

        public AuthorizationCodeProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STests(_conf, _ioc, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV1_Use_Fail_NotImplemented()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            var result = await _s2s.AuthorizationCodeV1(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV2_Use_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            var check = await _s2s.AuthorizationCodeV2(Guid.NewGuid().ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV2_Use_Fail_CodeInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = RandomNumber.CreateBase64(64);

            var result = await _s2s.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV2_Use_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            var result = await _s2s.AuthorizationCodeV2(client.Id.ToString(), Guid.NewGuid().ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV2_Use_Fail_UriInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var redirect = new Uri("https://app.test.net/a/invalid");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            var result = await _s2s.AuthorizationCodeV2(Guid.NewGuid().ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AuthCodeV2_Use_Success()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == BaseLib.Statics.ApiUnitTestUriALink).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.ContextStatus.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

            var result = await _s2s.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();

            check = JwtHelper.IsValidJwtFormat(refresh);
            check.Should().BeTrue();
        }
    }
}
