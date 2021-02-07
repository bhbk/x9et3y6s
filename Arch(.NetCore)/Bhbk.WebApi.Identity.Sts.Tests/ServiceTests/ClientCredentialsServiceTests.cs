using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
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
    public class ClientCredentialsServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ClientCredentialsServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Auth_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var cc = await service.Endpoints.ClientCredential_AuthV1(
                    new ClientCredentialV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "client_secret",
                        client_secret = AlphaNumeric.CreateString(8),
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Refresh_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var rt = await service.Endpoints.ClientCredential_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "refresh_token",
                        refresh_token = AlphaNumeric.CreateString(8),
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Auth_Fail_Client()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                audience.IsLockedOut = true;

                uow.Audiences.Update(audience);
                uow.Commit();

                var cc = await service.Endpoints.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = TestDefaultConstants.AudiencePassCurrent,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();

                var cc = await service.Endpoints.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "client_secret",
                        client_secret = TestDefaultConstants.AudiencePassCurrent,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Auth_Fail_Issuer()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var cc = await service.Endpoints.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = TestDefaultConstants.AudiencePassCurrent,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

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
                data.CreateAudienceRoles();

                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var cc = await service.Endpoints.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = TestDefaultConstants.AudiencePassCurrent,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();

                var result = await service.ClientCredential_GrantV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = audience.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = TestDefaultConstants.AudiencePassCurrent,
                    });
                result.Should().BeAssignableTo<ClientJwtV2>();

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

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Client()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                audience.IsLockedOut = true;

                uow.Audiences.Update(audience);
                uow.Commit();

                var result = await service.Endpoints.ClientCredential_RefreshV2(
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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ClientCredential_RefreshV2(
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
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Issuer()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                issuer.IsEnabled = false;

                uow.Issuers.Update(issuer);
                uow.Commit();

                var result = await service.Endpoints.ClientCredential_RefreshV2(
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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var result = await service.Endpoints.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Time()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                uow.Audiences.Clock = DateTime.UtcNow.AddYears(1);

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Audiences.Clock = DateTime.UtcNow;

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
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new StsService(conf, env.InstanceType, owin);

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateAudiences();
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                uow.Audiences.Clock = DateTime.UtcNow.AddYears(-1);

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                uow.Audiences.Clock = DateTime.UtcNow;

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
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Success()
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
                data.CreateAudienceRoles();

                var issuer = uow.Issuers.Get(x => x.Name == TestDefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == TestDefaultConstants.AudienceName).Single();

                var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
                var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

                uow.Refreshes.Create(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        RefreshType = ConsumerType.Client.ToString(),
                        RefreshValue = rt.RawData,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));
                uow.Commit();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.AccessExpire).Single();
                var result = await service.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { audience.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<ClientJwtV2>();

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
