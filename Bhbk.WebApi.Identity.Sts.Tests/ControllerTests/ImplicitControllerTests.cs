using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class ImplicitControllerTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;
        private readonly StsService _service;

        public ImplicitControllerTests(StartupTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new StsService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Auth_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            var imp = await _service.Http.Implicit_AuthV1(
                new ImplicitV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "implicit",
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = Base64.CreateString(8),
                    response_type = "token",
                    scope = Base64.CreateString(8),
                    state = Base64.CreateString(8),
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_ClientNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var state = Base64.CreateString(8);
            var imp = await _service.Http.Implicit_AuthV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    grant_type = "implicit",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_IssuerNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var state = Base64.CreateString(8);
            var imp = await _service.Http.Implicit_AuthV2(
                new ImplicitV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    grant_type = "implicit",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                });
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_UrlNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var state = Base64.CreateString(8);
            var imp = _service.Http.Implicit_AuthV2(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "implicit",
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                }).Result;
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_UserNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var state = Base64.CreateString(8);
            var imp = _service.Http.Implicit_AuthV2(
                new ImplicitV2()
                {
                    issuer = user.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "implicit",
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "token",
                    scope = "any",
                    state = state,
                }).Result;
            imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
            imp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Auth_Success()
        {
            var controller = new ImplicitController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var state = await uow.StateRepo.CreateAsync(
                _mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = Base64.CreateString(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                }));

            uow.CommitAsync().Wait();

            var imp = await controller.ImplicitV2_Auth(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "implicit",
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
             * implicit flow requires redirect with fragment in url. since the query parser library will 
             * not process a fragment, need to replace # with ? so can test values in redirect...
             */
            var imp_frag = "?" + imp_url.Fragment.Substring(1, imp_url.Fragment.Length - 1);

            HttpUtility.ParseQueryString(imp_frag).Get("state").Should().BeEquivalentTo(state.StateValue);
            HttpUtility.ParseQueryString(imp_frag).Get("grant_type").Should().BeEquivalentTo("implicit");
            HttpUtility.ParseQueryString(imp_frag).Get("token_type").Should().BeEquivalentTo("bearer");

            var result = HttpUtility.ParseQueryString(imp_frag).Get("access_token");

            JwtFactory.CanReadToken(result).Should().BeTrue();

            var jwt = JwtFactory.ReadJwtToken(result);

            var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            iss.Value.Split(':')[0].Should().Be(FakeConstants.ApiTestIssuer);
            iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

            var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                .Subtract(DateTime.UtcNow).TotalSeconds);
            exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
        }
    }
}
