using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    [Collection("StsTests")]
    public class AuthCodeControllerTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly StsRepository _endpoints;

        public AuthCodeControllerTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsRepository(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
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

            HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(Strings.ApiUnitTestUriLink);
            HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Client()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
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
            var ask_code = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = Strings.ApiUnitTestUriLink,
                    response_type = "code",
                    scope = "any",
                }) as RedirectResult;
            ask.Should().NotBeNull();
            ask.Should().BeAssignableTo(typeof(RedirectResult));
            ask.Permanent.Should().BeTrue();

            var ask_url = new Uri(ask.Url);
            var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
            var ask_code = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
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
            var ask_code = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
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
            var ask_code = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
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

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var controller = new AuthCodeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var ask = await controller.AskAuthCodeV2(
                new AuthCodeAskV2()
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
            var ask_code = await new ProtectHelper(_factory.UoW.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(_factory.UoW.ConfigRepo.AuthCodeTotpExpire), user);
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

            JwtHelper.CanReadToken(ac_check.access_token).Should().BeTrue();

            var ac_claims = JwtHelper.ReadJwtToken(ac_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            ac_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            ac_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
