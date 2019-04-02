using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTests")]
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

            var url = new Uri(Strings.ApiUnitTestUri1Link);

            var ask = await _endpoints.AuthorizationCode_AskV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Generate_NotImplemented()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = new Uri(Strings.ApiUnitTestUri2Link);

            var access = await _endpoints.AuthorizationCode_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, RandomValues.CreateBase64String(8), RandomValues.CreateBase64String(8));

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = new Uri(Strings.ApiUnitTestUri1Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), Guid.NewGuid().ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            url = new Uri("https://app.test.net/a/invalid");

            ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = new Uri(Strings.ApiUnitTestUri1Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(Guid.NewGuid().ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = Guid.NewGuid();

            var url = new Uri(Strings.ApiUnitTestUri1Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var url = new Uri(Strings.ApiUnitTestUri1Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = new Uri(JsonConvert.DeserializeObject<string>(await ask.Content.ReadAsStringAsync()));

            HttpUtility.ParseQueryString(ok.Query).Get("authorization_code").Should().NotBeNullOrEmpty();
            HttpUtility.ParseQueryString(ok.Query).Get("redirect_uri").Should().NotBeNullOrEmpty();
            HttpUtility.ParseQueryString(ok.Query).Get("state").Should().NotBeNullOrEmpty();

            ////not done yet...
            //var pairs = ask.Headers.GetValues("Set-Cookie");

            //var cookies = new CookieContainer();
            //var handler = new HttpClientHandler();

            //handler.CookieContainer = cookies;

            //var http = new HttpClient(handler);
            //var response = ask;

            //var uri = new Uri("http://google.com");
            //var responseCookies = cookies.GetCookies(uri).Cast<Cookie>();

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

            var url = new Uri(Strings.ApiUnitTestUri2Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ask_ok = new Uri(JsonConvert.DeserializeObject<string>(await ask.Content.ReadAsStringAsync()));
            var ask_code = HttpUtility.ParseQueryString(ask_ok.Query).Get("authorization_code");
            var ask_redirect = HttpUtility.ParseQueryString(ask_ok.Query).Get("redirect_uri");
            var ask_state = HttpUtility.ParseQueryString(ask_ok.Query).Get("state");

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            await _factory.UoW.CommitAsync();

            var access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                ask_redirect, ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = new Uri(Strings.ApiUnitTestUri2Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, "any");

            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ask_ok = new Uri(JsonConvert.DeserializeObject<string>(await ask.Content.ReadAsStringAsync()));
            var ask_code = HttpUtility.ParseQueryString(ask_ok.Query).Get("authorization_code");
            var ask_redirect = HttpUtility.ParseQueryString(ask_ok.Query).Get("redirect_uri");
            var ask_state = HttpUtility.ParseQueryString(ask_ok.Query).Get("state");

            var access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, RandomValues.CreateBase64String(64), ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                RandomValues.CreateBase64String(64), ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            await _factory.UoW.CommitAsync();

            access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var url = new Uri(Strings.ApiUnitTestUri2Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ask_ok = new Uri(JsonConvert.DeserializeObject<string>(await ask.Content.ReadAsStringAsync()));
            var ask_code = HttpUtility.ParseQueryString(ask_ok.Query).Get("authorization_code");
            var ask_redirect = HttpUtility.ParseQueryString(ask_ok.Query).Get("redirect_uri");
            var ask_state = HttpUtility.ParseQueryString(ask_ok.Query).Get("state");

            var access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), Guid.NewGuid().ToString(), 
                url.AbsoluteUri, ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Generate_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var url = new Uri(Strings.ApiUnitTestUri2Link);

            var ask = await _endpoints.AuthorizationCode_AskV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), url.AbsoluteUri, "any");
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.OK);

            var ask_ok = new Uri(JsonConvert.DeserializeObject<string>(await ask.Content.ReadAsStringAsync()));
            var ask_code = HttpUtility.ParseQueryString(ask_ok.Query).Get("authorization_code");
            var ask_redirect = HttpUtility.ParseQueryString(ask_ok.Query).Get("redirect_uri");
            var ask_state = HttpUtility.ParseQueryString(ask_ok.Query).Get("state");

            var access = await _endpoints.AuthorizationCode_GenerateV2(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), 
                url.AbsoluteUri, ask_code, ask_state);

            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var access_ok = JObject.Parse(await access.Content.ReadAsStringAsync());
            var access_check = access_ok.ToObject<UserJwtV2>();
            access_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(access_check.access_token).Should().BeTrue();

            var access_claims = JwtBuilder.ReadJwtToken(access_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            access_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            access_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
