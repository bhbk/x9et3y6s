using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interop;
using Bhbk.Lib.Identity.Primitives;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class AuthorizationCodeProviderTest : StartupTest
    {
        private TestServer _owin;
        private StsTester _sts;

        public AuthorizationCodeProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _sts = new StsTester(_conf, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV1_Use_Fail_NotImplemented()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.Status.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigStore.Values.DefaultsAuthorizationCodeExpire), user));

            var result = await _sts.AuthorizationCodeV1(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV2_Use_Fail_ClientNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.Status.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigStore.Values.DefaultsAuthorizationCodeExpire), user));

            _ioc.ClientMgmt.Store.Delete(client);

            var check = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV2_Use_Fail_CodeNotValid()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = RandomValues.CreateBase64String(64);

            var result = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV2_Use_Fail_UriNotValid()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var redirect = new Uri("https://app.test.net/a/invalid");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.Status.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigStore.Values.DefaultsAuthorizationCodeExpire), user));

            var result = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), RandomValues.CreateBase64String(64), code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV2_Use_Fail_UserNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.Status.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigStore.Values.DefaultsAuthorizationCodeExpire), user));

            _ioc.UserMgmt.Store.DeleteAsync(user).Wait();

            var result = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AuthCodeV2_Use_Success()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = audience.AppAudienceUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_ioc.Status.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(_ioc.ConfigStore.Values.DefaultsAuthorizationCodeExpire), user));

            var result = await _sts.AuthorizationCodeV2(client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();

            check = JwtSecureProvider.IsValidJwtFormat(refresh);
            check.Should().BeTrue();
        }
    }
}
