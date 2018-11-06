using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
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
    [Collection("StsTestCollection")]
    public class AuthorizationCodeProviderTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public AuthorizationCodeProviderTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Request_Fail_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_RequestV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Use_Fail_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single();

            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Request_Fail_ClientNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_RequestV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Request_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_RequestV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Request_Fail_UriNotValid()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = new Uri("https://app.test.net/a/invalid");

            var result = await _endpoints.AuthorizationCode_RequestV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Request_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = Guid.NewGuid();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_RequestV2(issuer.Id.ToString(), client.Id.ToString(), user.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_AuthCodeV2_Request_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_RequestV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
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

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single();

            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer);
            await _factory.UoW.CommitAsync();

            var check = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_CodeNotValid()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single();

            var redirect = new Uri(url.AbsoluteUri);
            var code = RandomValues.CreateBase64String(64);

            var result = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_UriNotValid()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var redirect = new Uri("https://app.test.net/a/invalid");
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), RandomValues.CreateBase64String(64), code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            await _factory.UoW.UserRepo.DeleteAsync(user);
            await _factory.UoW.CommitAsync();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single();

            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single();

            var redirect = new Uri(url.AbsoluteUri);
            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.PasswordHash, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtProvider.CanReadToken(access);
            check.Should().BeTrue();

            check = JwtProvider.CanReadToken(refresh);
            check.Should().BeTrue();
        }
    }
}
