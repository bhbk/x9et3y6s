using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class ImplicitControllerTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public ImplicitControllerTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Auth_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            var imp = await service.Http.Implicit_AuthV1(
                new ImplicitV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "implicit",
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
        public async Task Sts_OAuth2_ImplicitV2_Auth_Fail_ClientNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var state = RandomValues.CreateBase64String(8);
            var imp = await service.Http.Implicit_AuthV2(
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
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var state = RandomValues.CreateBase64String(8);
            var imp = await service.Http.Implicit_AuthV2(
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
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var state = RandomValues.CreateBase64String(8);
            var imp = service.Http.Implicit_AuthV2(
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
            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();
            var service = new StsService(uow.InstanceType, _owin);

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var url = new Uri(Constants.ApiUnitTestUriLink);
            var state = RandomValues.CreateBase64String(8);
            var imp = service.Http.Implicit_AuthV2(
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
            var controller = new ImplicitController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

            new TestData(uow).DestroyAsync().Wait();
            new TestData(uow).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
            var url = new Uri(Constants.ApiUnitTestUriLink);
            var state = await uow.StateRepo.CreateAsync(
                uow.Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
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
            iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
            iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

            var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                .Subtract(DateTime.UtcNow).TotalSeconds);
            exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
        }
    }
}
