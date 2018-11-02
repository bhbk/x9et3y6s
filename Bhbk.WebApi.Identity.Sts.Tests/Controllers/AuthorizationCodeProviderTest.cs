using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
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
        public async Task Api_Sts_OAuth2_AuthCodeV1_Use_Fail_NotImplemented()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _sts.AuthorizationCodeV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AuthCodeV2_Use_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            await _uow.IssuerRepo.DeleteAsync(issuer);

            var check = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AuthCodeV2_Use_Fail_CodeNotValid()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = RandomValues.CreateBase64String(64);

            var result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AuthCodeV2_Use_Fail_UriNotValid()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var redirect = new Uri("https://app.test.net/a/invalid");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), RandomValues.CreateBase64String(64), code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AuthCodeV2_Use_Fail_UserNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            _uow.CustomUserMgr.Store.DeleteAsync(user).Wait();

            var result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AuthCodeV2_Use_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();

            check = JwtSecureProvider.CanReadToken(refresh);
            check.Should().BeTrue();
        }
    }
}
