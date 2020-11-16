using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    public class ResourceOwnerServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ResourceOwnerServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                audience.IsEnabled = false;

                uow.Audiences.Update(audience);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.UserName,
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = AlphaNumeric.CreateString(8),
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                user.IsLockedOut = true;
                user.LockoutEndUtc = DateTime.UtcNow.AddSeconds(60);

                uow.Users.Update(user);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = Guid.NewGuid().ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();

                var result = await service.ResourceOwner_GrantV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV1>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "true";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.ResourceOwner_GrantV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = audience.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV1Legacy>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Should().Be(Constants.TestIssuer);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(86399, 86400);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                audience.IsEnabled = false;

                uow.Audiences.Update(audience);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                user.IsLockedOut = true;
                user.LockoutEndUtc = DateTime.UtcNow.AddSeconds(60);

                uow.Users.Update(user);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Users.Delete(user);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Fail_Time()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                uow.Users.Clock = DateTime.UtcNow.AddYears(1);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Users.Clock = DateTime.UtcNow;

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                uow.Users.Clock = DateTime.UtcNow.AddYears(-1);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Users.Clock = DateTime.UtcNow;

                var result = await service.Endpoints.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();
                var result = await service.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = audience.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV1>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                audience.IsEnabled = false;

                uow.Audiences.Update(audience);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = AlphaNumeric.CreateString(8),
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                user.IsLockedOut = true;
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(60);

                uow.Users.Update(user);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var result = await service.Endpoints.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = Guid.NewGuid().ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();
                var result = await service.ResourceOwner_GrantV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var defaultAudience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var defaultRole = uow.Roles.Get(x => x.Name == Constants.DefaultRoleForUser_Identity).Single();

                if (audience.Id == defaultAudience.Id)
                    throw new ArgumentException();

                uow.Users.AddToRole(user, defaultRole);
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();

                var result = await service.ResourceOwner_GrantV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString(), defaultAudience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();
                var result = await service.ResourceOwner_GrantV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.TestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                audience.IsEnabled = false;

                uow.Audiences.Update(audience);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Fail_Time()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                uow.Users.Clock = DateTime.UtcNow.AddYears(1);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Users.Clock = DateTime.UtcNow;

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                uow.Users.Clock = DateTime.UtcNow.AddYears(-1);

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Users.Clock = DateTime.UtcNow;

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                user.IsLockedOut = true;
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(60);

                uow.Users.Update(user);
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = (string)rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();
                var result = await service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var defaultAudience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var defaultRole = uow.Roles.Get(x => x.Name == Constants.DefaultRoleForUser_Identity).Single();

                if (audience.Id == defaultAudience.Id)
                    throw new ArgumentException();

                uow.Users.AddToRole(user, defaultRole);
                uow.Commit();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();

                var result = await service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString(), defaultAudience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(instance.InstanceType, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

                legacyIssuer.ConfigValue = "false";

                uow.Settings.Update(legacyIssuer);
                uow.Commit();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Create(
                    mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingAccessExpire).Single();
                var result = await service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Name }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                auth.Valid(result.access_token).Should().BeTrue();

                var jwt = auth.Parse(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.TestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
