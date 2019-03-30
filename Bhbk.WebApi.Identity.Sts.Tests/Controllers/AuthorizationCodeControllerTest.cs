using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
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
    public class AuthorizationCodeControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public AuthorizationCodeControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_AskV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Generate_NotImplemented()
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
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_UseV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single().AbsoluteUri;

            var result = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            url = new Uri("https://app.test.net/a/invalid").AbsoluteUri;

            result = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_AskV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = Guid.NewGuid();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.ToString(), url.AbsoluteUri, "all");
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri1Link).Single();

            var result = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "all");
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
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single().AbsoluteUri;

            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_UseV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single().AbsoluteUri;

            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url, RandomValues.CreateBase64String(64));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), RandomValues.CreateBase64String(64), code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = (await _factory.UoW.ClientRepo.GetUriListAsync(client.Id))
                .Where(x => x.AbsoluteUri == Strings.ApiUnitTestUri2Link).Single().AbsoluteUri;

            var code = HttpUtility.UrlEncode(await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), Guid.NewGuid().ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Success()
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
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(UInt32.Parse(_factory.Conf["IdentityDefaults:AuthorizationCodeExpire"])), user));

            var result = await _endpoints.AuthorizationCode_UseV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), redirect.AbsoluteUri, code);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtBuilder.CanReadToken(access);
            check.Should().BeTrue();

            check = JwtBuilder.CanReadToken(refresh);
            check.Should().BeTrue();
        }
    }
}
