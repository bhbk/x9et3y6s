using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class AuthCodeControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public AuthCodeControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            var ask = await _endpoints.AuthCode_AskV1(
                new AuthCodeRequestV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "code",
                    scope = RandomValues.CreateBase64String(8),
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Use_NotImplemented()
        {
            var ac = await _endpoints.AuthCode_UseV1(
                new AuthCodeV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = RandomValues.CreateBase64String(8),
                    state = RandomValues.CreateBase64String(8),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
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
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);

            ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeRequestV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
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
            var ask = await _endpoints.AuthCode_AskV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
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

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUri1Link);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);

            HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(Strings.ApiUnitTestUri1Link);
            HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUri2Link);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), user);
            var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

            var ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = RandomValues.CreateBase64String(8),
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            await _factory.UoW.CommitAsync();

            ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Issuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = Strings.ApiUnitTestUri2Link,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), user);
            var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

            await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            await _factory.UoW.CommitAsync();

            var ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = ask_redirect,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_User()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUri2Link);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), user);
            var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

            var ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Validate()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUri2Link);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), user);
            var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

            var ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = ask_redirect,
                    code = ask_code,
                    state = RandomValues.CreateBase64String(32),
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = ask_redirect,
                    code = RandomValues.CreateBase64String(32),
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer2)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUri2Link);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeRequestV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectProvider(_factory.UoW.Situation.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), user);
            var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

            var ac = await _endpoints.AuthCode_UseV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = ask_redirect,
                    code = ask_code,
                    state = ask_state,
                });

            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.OK);

            var ac_ok = JObject.Parse(await ac.Content.ReadAsStringAsync());
            var ac_check = ac_ok.ToObject<UserJwtV2>();
            ac_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(ac_check.access_token).Should().BeTrue();

            var ac_claims = JwtBuilder.ReadJwtToken(ac_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            ac_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer2);
            ac_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
