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
    public class AuthCodeControllerTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IContextService _instance;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;
        private readonly StsService _service;

        public AuthCodeControllerTests(StartupTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _instance = _factory.Server.Host.Services.GetRequiredService<IContextService>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new StsService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            var ask = await _service.Http.AuthCode_AskV1(
                new AuthCodeAskV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    response_type = "code",
                    scope = Base64.CreateString(8),
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Auth_NotImplemented()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            var ac = await _service.Http.AuthCode_AuthV1(
                new AuthCodeV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "authorization_code",
                    username = Guid.NewGuid().ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = Base64.CreateString(8),
                    state = Base64.CreateString(8),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_ClientNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var ask = await _service.Http.AuthCode_AskV2(
                new AuthCodeAskV2()
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_IssuerNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var ask = await _service.Http.AuthCode_AskV2(
                new AuthCodeAskV2()
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
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UserNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var ask = await _service.Http.AuthCode_AskV2(
                new AuthCodeAskV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    response_type = "code",
                    scope = "any",
                });
            ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UrlNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var ask = await _service.Http.AuthCode_AskV2(
                new AuthCodeAskV2()
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
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            var controller = new AuthCodeController(_conf, _instance);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var url = new Uri(FakeConstants.ApiTestUriLink);
            var ask = await controller.AuthCodeV2_Ask(
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

            HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(FakeConstants.ApiTestUriLink);
            HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_ClientNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_IssuerNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UrlNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserNotExist()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = Guid.NewGuid().ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidCode()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = Base64.CreateString(32),
                    state = model.StateValue,
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidState()
        {
            var uow = _factory.Server.Host.Services.GetRequiredService<IUoWService>();

            new TestData(uow, _mapper).DestroyAsync().Wait();
            new TestData(uow, _mapper).CreateAsync().Wait();

            var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
            var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
            var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

            var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);
            var url = new Uri(FakeConstants.ApiTestUriLink);
            var model = await uow.StateRepo.CreateAsync(
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

            var ac = await _service.Http.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = url.AbsoluteUri,
                    code = code,
                    state = Base64.CreateString(32),
                });
            ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
            ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Success()
        {
            var controller = new AuthCodeController(_conf, _instance);
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
                && x.ConfigKey == RealConstants.ApiSettingTotpExpire)).Single();

            var ask = uow.StateRepo.CreateAsync(
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
                })).Result;
            var ask_url = new Uri(FakeConstants.ApiTestUriLink);
            var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

            uow.CommitAsync().Wait();

            var result = _service.AuthCode_AuthV2(
                new AuthCodeV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "authorization_code",
                    user = user.Id.ToString(),
                    redirect_uri = ask_url.AbsoluteUri,
                    code = ask_code,
                    state = ask.StateValue,
                });
            result.Should().BeAssignableTo<UserJwtV2>();

            JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

            var jwt = JwtFactory.ReadJwtToken(result.access_token);

            var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            iss.Value.Split(':')[0].Should().Be(FakeConstants.ApiTestIssuer);
            iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

            var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                .Subtract(DateTime.UtcNow).TotalSeconds);
            exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
        }
    }
}
