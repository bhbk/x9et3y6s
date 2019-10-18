using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

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
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                var cc = await service.Http.ClientCredential_AuthV1(
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
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                var rt = await service.Http.ClientCredential_RefreshV1(
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                client.Enabled = false;

                uow.Clients.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var cc = await service.Http.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = client.ClientKey,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = await service.Http.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        grant_type = "client_secret",
                        client_secret = client.ClientKey,
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                issuer.Enabled = false;

                uow.Issuers.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var cc = await service.Http.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = client.ClientKey,
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = await service.Http.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = client.ClientKey,
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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var expire = (await uow.Settings.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();

                var result = service.ClientCredential_AuthV2(
                    new ClientCredentialV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        grant_type = "client_secret",
                        client_secret = client.ClientKey,
                    });
                result.Should().BeAssignableTo<ClientJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(FakeConstants.ApiTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.Issuers.Salt);

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
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                client.Enabled = false;

                uow.Clients.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var rt = await service.Http.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                var rt = await service.Http.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                issuer.Enabled = false;

                uow.Issuers.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var rt = await service.Http.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                var rt = await service.Http.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Time()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                uow.Clients.Clock = DateTime.UtcNow.AddYears(1);

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                uow.Clients.Clock = DateTime.UtcNow;

                var rt = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                uow.Clients.Clock = DateTime.UtcNow.AddYears(-1);

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                uow.Clients.Clock = DateTime.UtcNow;

                var rt = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new StsService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, mapper, issuer, client).Result;
                uow.CommitAsync().Wait();

                var expire = (await uow.Settings.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();
                var result = service.ClientCredential_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = cc.RawData,
                    });
                result.Should().BeAssignableTo<ClientJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(FakeConstants.ApiTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.Issuers.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
