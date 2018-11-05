using Bhbk.Lib.Identity.Data;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class OAuth2ControllerTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly StsClient _sts;

        public OAuth2ControllerTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _sts = new StsClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_GetList_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_GetList_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeAll_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenDeleteAllV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeAll_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteAllV1(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeOne_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV1(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV1_RevokeOne_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV1(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_GetList_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenGetListV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeAll_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];

            var result = await _sts.RefreshTokenDeleteAllV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeAll_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteAllV2(new JwtSecurityToken(bearer), user.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeOne_Fail_Auth()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV2(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sts_OAuth2_RefreshV2_RevokeOne_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();
            new DefaultData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Email, Strings.ApiDefaultUserPassword);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var bearer = (string)jwt["access_token"];
            var refresh = user.AppUserRefresh.Where(x => x.UserId == user.Id).Single();

            var result = await _sts.RefreshTokenDeleteV2(new JwtSecurityToken(bearer), user.Id.ToString(), refresh.Id.ToString());
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh.ProtectedTicket);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_RequestCodeV1_Fail_NotImplemented()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_RequestCodeV2_Fail_ClientNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RequestCodeV2_Fail_IssuerNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RequestCodeV2_Fail_UriNotValid()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = new Uri("https://app.test.net/a/invalid");

            var result = await _sts.AuthorizationCodeRequestV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_RequestCodeV2_Fail_UserNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = Guid.NewGuid();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(issuer.Id.ToString(), client.Id.ToString(), user.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "Not Implemented")]
        public async Task Sts_OAuth2_RequestCodeV2_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _sts.AuthorizationCodeRequestV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
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
