﻿using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    public class DeviceCodeServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public DeviceCodeServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Ask_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var ask = await service.Endpoints.DeviceCode_AskV1(
                    new DeviceCodeAskV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        username = Guid.NewGuid().ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV1_Auth_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var dc = await service.Endpoints.DeviceCode_AuthV1(
                    new DeviceCodeV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user_code = Base64.CreateString(32),
                        device_code = Base64.CreateString(32),
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var ask = await service.Endpoints.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var ask = await service.Endpoints.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var ask = await service.Endpoints.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user = Guid.NewGuid().ToString(),
                    });
                ask.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ask.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Ask_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var ask = await service.DeviceCode_AskV2(
                    new DeviceCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user = user.Id.ToString(),
                    });
                ask.Should().BeAssignableTo<DeviceCodeV2>();
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var secret = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user.Id.ToString());

                var state = uow.States.Create(
                    map.Map<tbl_State>(new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = ConsumerType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Endpoints.DeviceCode_AuthV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_DeviceCode()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var secret = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user.Id.ToString());

                var state = uow.States.Create(
                    map.Map<tbl_State>(new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = ConsumerType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Endpoints.DeviceCode_AuthV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = AlphaNumeric.CreateString(32),
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var secret = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user.Id.ToString());

                var state = uow.States.Create(
                    map.Map<tbl_State>(new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = ConsumerType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Endpoints.DeviceCode_AuthV2(
                    new DeviceCodeV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Fail_UserCode()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var secret = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user.Id.ToString());

                var state = uow.States.Create(
                    map.Map<tbl_State>(new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = ConsumerType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Endpoints.DeviceCode_AuthV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user_code = AlphaNumeric.CreateString(32),
                        device_code = state.StateValue,
                    });
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_DeviceCodeV2_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();
                var user = uow.Users.Get(x => x.UserName == TestDefaultConstants.UserName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var secret = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user.Id.ToString());

                var state = uow.States.Create(
                    map.Map<tbl_State>(new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = ConsumerType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                /*
                 * the state needs to be approved... would be done by user...
                 */
                state.StateDecision = true;

                uow.States.Update(state);
                uow.Commit();

                var result = await service.DeviceCode_GrantV2(
                    new DeviceCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "device_code",
                        user_code = secret,
                        device_code = state.StateValue,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(TestDefaultConstants.IssuerName);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenant:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
