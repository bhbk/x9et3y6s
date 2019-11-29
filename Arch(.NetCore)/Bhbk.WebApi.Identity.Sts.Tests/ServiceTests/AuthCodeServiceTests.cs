﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    public class AuthCodeServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public AuthCodeServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Ask_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                var ask = await service.Http.AuthCode_AskV1(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV1_Auth_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                var ac = await service.Http.AuthCode_AuthV1(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_ClientNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var ask = await service.Http.AuthCode_AskV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_IssuerNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var ask = await service.Http.AuthCode_AskV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UserNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var ask = await service.Http.AuthCode_AskV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Fail_UrlNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var ask = await service.Http.AuthCode_AskV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_ClientNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var ac = await service.Http.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "authorization_code",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = code,
                        state = state.StateValue,
                    });
                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_IssuerNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var ac = await service.Http.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
                        grant_type = "authorization_code",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = code,
                        state = state.StateValue,
                    });
                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UrlNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var ac = await service.Http.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "authorization_code",
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        code = code,
                        state = state.StateValue,
                    });
                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserNotExist()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var ac = await service.Http.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "authorization_code",
                        user = Guid.NewGuid().ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = code,
                        state = state.StateValue,
                    });
                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidCode()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var ac = await service.Http.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "authorization_code",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = Base64.CreateString(32),
                        state = state.StateValue,
                    });
                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Fail_UserInvalidState()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var ac = await service.Http.AuthCode_AuthV2(
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
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).Destroy();
                new TestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == FakeConstants.ApiTestIssuer).Single();
                var client = uow.Clients.Get(x => x.Name == FakeConstants.ApiTestClient).Single();
                var user = uow.Users.Get(x => x.Email == FakeConstants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingTotpExpire).Single();

                var url = new Uri(FakeConstants.ApiTestUriLink);

                var code = new PasswordlessTokenFactory(uow.InstanceType.ToString())
                    .Generate(user.SecurityStamp, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.ClientId == client.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

                var result = await service.AuthCode_AuthV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "authorization_code",
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = code,
                        state = state.StateValue,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = auth.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(FakeConstants.ApiTestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}