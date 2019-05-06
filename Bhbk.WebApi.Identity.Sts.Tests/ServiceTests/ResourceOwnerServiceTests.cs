using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
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
    public class ResourceOwnerServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public ResourceOwnerServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_Issuer()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Fail_User()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = RandomValues.CreateBase64String(8),
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

                uow.UserRepo.UpdateAsync(user).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = Guid.NewGuid().ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Auth_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_AuthV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV1>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "true";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.ResourceOwner_AuthV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV1Legacy>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Should().Be(Constants.ApiUnitTestIssuer);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(86399, 86400);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = client.Id.ToString(),
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

                uow.UserRepo.UpdateAsync(user).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                uow.UserRepo.DeleteAsync(user.Id).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                uow.UserRepo.Clock = DateTime.UtcNow.AddYears(1);

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                uow.UserRepo.Clock = DateTime.UtcNow;

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                uow.UserRepo.Clock = DateTime.UtcNow.AddYears(-1);

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                uow.UserRepo.Clock = DateTime.UtcNow;

                var result = await service.Http.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = await JwtFactory.UserRefreshV1(uow, issuer, user);
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_RefreshV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV1>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var rop = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_Issuer()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Fail_User()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = RandomValues.CreateBase64String(8),
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

                uow.UserRepo.UpdateAsync(user).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "password",
                        user = Guid.NewGuid().ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Auth_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var defaultClient = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var defaultRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).Single();

                if (client.Id == defaultClient.Id)
                    throw new ArgumentException();

                uow.UserRepo.AddRoleAsync(user, defaultRole).Wait();
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString(), defaultClient.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_AuthV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Name }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_Fail_Client()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                client.Enabled = false;

                uow.ClientRepo.UpdateAsync(client).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                issuer.Enabled = false;

                uow.IssuerRepo.UpdateAsync(issuer).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                uow.UserRepo.Clock = DateTime.UtcNow.AddYears(1);

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                uow.UserRepo.Clock = DateTime.UtcNow;

                var result = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                uow.UserRepo.Clock = DateTime.UtcNow.AddYears(-1);

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                uow.UserRepo.Clock = DateTime.UtcNow;

                var result = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

                uow.UserRepo.UpdateAsync(user).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = (string)rt.RawData,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var result = await service.Http.ResourceOwner_RefreshV2(
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
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var defaultClient = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var defaultRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).Single();

                if (client.Id == defaultClient.Id)
                    throw new ArgumentException();

                uow.UserRepo.AddRoleAsync(user, defaultRole).Wait();
                uow.CommitAsync().Wait();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString(), defaultClient.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new StsService(uow.InstanceType, _owin);

                new TestData(uow).DestroyAsync().Wait();
                new TestData(uow).CreateAsync().Wait();

                var legacyIssuer = (uow.SettingRepo.GetAsync(x => x.IssuerId == null && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiDefaultSettingLegacyIssuer)).Result.Single();

                legacyIssuer.ConfigValue = "false";

                uow.SettingRepo.UpdateAsync(legacyIssuer).Wait();
                uow.CommitAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rt = JwtFactory.UserRefreshV2(uow, issuer, user).Result;
                uow.CommitAsync().Wait();

                var expire = (await uow.SettingRepo.GetAsync(x => x.ConfigKey == Constants.ApiDefaultSettingExpireAccess)).Single();
                var result = service.ResourceOwner_RefreshV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Name }),
                        grant_type = "refresh_token",
                        refresh_token = rt.RawData,
                    });
                result.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(result.access_token).Should().BeTrue();

                var jwt = JwtFactory.ReadJwtToken(result.access_token);

                var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                iss.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                iss.Value.Split(':')[1].Should().Be(uow.IssuerRepo.Salt);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
