using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class ImplicitServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly IStsService _service;

        public ImplicitServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new StsService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Use_NotImplemented()
        {
            var imp = await _service.Repo.Implicit_UseV1(
                new ImplicitV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = RandomValues.CreateBase64String(8),
                    response_type = "token",
                    scope = RandomValues.CreateBase64String(8),
                    state = RandomValues.CreateBase64String(8),
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Use_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var url = new Uri(Strings.ApiUnitTestUriLink);
            var state = RandomValues.CreateBase64String(8);
            var imp = await _service.Repo.Implicit_UseV2(
                new ImplicitV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            imp = await _service.Repo.Implicit_UseV2(
                new ImplicitV2()
                {
                    issuer = string.Empty,
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            imp = await _service.Repo.Implicit_UseV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            imp = await _service.Repo.Implicit_UseV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            imp = await _service.Repo.Implicit_UseV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Empty,
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var controller = new ImplicitController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var url = new Uri(Strings.ApiUnitTestUriLink);

            var state = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(_factory.UoW.ConfigRepo.ImplicitTokenExpire),
                });

            await _factory.UoW.CommitAsync();

            var imp = await controller.UseImplicitV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state.StateValue,
                }) as RedirectResult;
            imp.Should().BeAssignableTo(typeof(RedirectResult));
            imp.Permanent.Should().BeTrue();

            var imp_url = new Uri(imp.Url);
            var imp_uri = imp_url.AbsoluteUri.Substring(0, imp_url.AbsoluteUri.IndexOf('#'));

            imp_uri.Should().BeEquivalentTo(url.AbsoluteUri);

            /*
             * implicit flow requires redirect with fragment in url. since the query parser library will not 
             * process a fragment, need to replace # with ? so can test values in redirect...
             */
            var imp_frag = "?" + imp_url.Fragment.Substring(1, imp_url.Fragment.Length - 1);

            HttpUtility.ParseQueryString(imp_frag).Get("state").Should().BeEquivalentTo(state.StateValue);
            HttpUtility.ParseQueryString(imp_frag).Get("grant_type").Should().BeEquivalentTo("implicit");
            HttpUtility.ParseQueryString(imp_frag).Get("token_type").Should().BeEquivalentTo("bearer");

            var imp_rop = HttpUtility.ParseQueryString(imp_frag).Get("access_token");

            JwtHelper.CanReadToken(imp_rop).Should().BeTrue();

            var imp_claims = JwtHelper.ReadJwtToken(imp_rop).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            imp_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            imp_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
