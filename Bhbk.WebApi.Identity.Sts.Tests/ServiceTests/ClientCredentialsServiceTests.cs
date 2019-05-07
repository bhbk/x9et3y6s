using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Helpers;
using Bhbk.Lib.Identity.Data.Infrastructure;
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
    public class ClientCredentialsServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly StsService _service;

        public ClientCredentialsServiceTests(StartupTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();

            _service = new StsService(conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Auth_NotImplemented()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                var cc = await _service.Http.ClientCredential_AuthV1(
                    new ClientCredentialV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "client_secret",
                        client_secret = RandomValues.CreateAlphaNumericString(8),
                    });
                cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                cc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Refresh_NotImplemented()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                var rt = await _service.Http.ClientCredential_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "refresh_token",
                        refresh_token = RandomValues.CreateAlphaNumericString(8),
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Auth_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var cc = await _service.Http.ClientCredential_AuthV2(
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

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = await _service.Http.ClientCredential_AuthV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var cc = await _service.Http.ClientCredential_AuthV2(
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

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = await _service.Http.ClientCredential_AuthV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();
                var result = _service.ClientCredential_AuthV2(
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
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var rt = await _service.Http.ClientCredential_RefreshV2(
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

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                var rt = await _service.Http.ClientCredential_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var rt = await _service.Http.ClientCredential_RefreshV2(
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

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                var rt = await _service.Http.ClientCredential_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                uow.ClientRepo.Clock = DateTime.UtcNow.AddYears(1);

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                uow.ClientRepo.Clock = DateTime.UtcNow;

                var rt = await _service.Http.ResourceOwner_RefreshV2(
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

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                uow.ClientRepo.Clock = DateTime.UtcNow.AddYears(-1);

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                uow.ClientRepo.Clock = DateTime.UtcNow;

                var rt = await _service.Http.ResourceOwner_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IUnitOfWork>();

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == FakeConstants.ApiTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var cc = JwtFactory.ClientRefreshV2(uow, issuer, client).Result;
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();
                var result = _service.ClientCredential_RefreshV2(
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
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
