using Bhbk.Lib.Core.Cryptography;
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
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("NoParallelExecute")]
    public class AuthorizationCodeProviderTest : IClassFixture<StartupTest>
    {
        private readonly HttpClient _client;
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly StsClient _sts;

        public AuthorizationCodeProviderTest(StartupTest fake)
        {
            _client = fake.CreateClient();
            _conf = fake.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
            _uow = fake.Server.Host.Services.GetRequiredService<IIdentityContext<AppDbContext>>();
            _sts = new StsClient(_conf, _uow.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Use_Fail_NotImplemented()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_IssuerNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            await _uow.IssuerRepo.DeleteAsync(issuer);
            await _uow.CommitAsync();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var check = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_CodeNotValid()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_UriNotValid()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_UserNotFound()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            await _uow.CustomUserMgr.Store.DeleteAsync(user);
            await _uow.CommitAsync();

            var url = client.AppClientUri.Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();
            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_uow.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _sts.AuthorizationCodeV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Success()
        {
            new TestData(_uow).Destroy();
            new TestData(_uow).Create();

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
