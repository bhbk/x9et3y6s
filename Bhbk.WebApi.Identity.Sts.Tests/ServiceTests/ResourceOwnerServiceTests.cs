using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
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
    [Collection("StsTests")]
    public class ResourceOwnerServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly IStsService _service;

        public ResourceOwnerServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new StsService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = string.Empty,
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = string.Empty,
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            /*
             */
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);
            await _factory.UoW.CommitAsync();

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailToken()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            var pos = random.Next((rop.refresh_token).Length - 8);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = (rop.refresh_token).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)),
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            /*
             */
            rt = await _service.Raw.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = string.Empty,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = string.Empty,
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailClient_Legacy()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = true;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = Guid.NewGuid().ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = string.Empty,
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = string.Empty,
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = Guid.NewGuid().ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = string.Empty,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = RandomValues.CreateBase64String(8),
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);

            rop = await _service.Raw.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailUser_Legacy()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = true;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = Guid.NewGuid().ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = string.Empty,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = RandomValues.CreateBase64String(8),
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);

            rop = await _service.Raw.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            /*
             */
            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var rop = _service.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo<UserJwtV1>();

            JwtHelper.CanReadToken(rop.access_token).Should().BeTrue();

            var rop_claims = JwtHelper.ReadJwtToken(rop.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rop_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            rop_claims.Value.Split(':')[1].Should().Be(salt);

            var rt = _service.ResourceOwnerRefresh_UseV1(
                new RefreshTokenV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo<UserJwtV1>();

            JwtHelper.CanReadToken(rt.access_token).Should().BeTrue();

            var rt_claims = JwtHelper.ReadJwtToken(rt.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            rt_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            _factory.UoW.ConfigRepo.LegacyModeIssuer = true;

            var rop_legacy = _service.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop_legacy.Should().BeAssignableTo<UserJwtV1Legacy>();

            JwtHelper.CanReadToken(rop_legacy.access_token).Should().BeTrue();

            var rop_legacy_claims = JwtHelper.ReadJwtToken(rop_legacy.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rop_legacy_claims.Value.Should().Be(Strings.ApiUnitTestIssuer);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = string.Empty,
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailToken()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var random = new Random();
            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            var rop_pos = random.Next((rop.refresh_token).Length - 8);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (rop.refresh_token).Remove(rop_pos, 8).Insert(rop_pos, RandomValues.CreateBase64String(8)),
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            /*
             */
            rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = string.Empty,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);
            await _factory.UoW.CommitAsync();

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (string)rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await _factory.UoW.UserRepo.DeleteAsync(user.Id);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = rop.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString(), RandomValues.CreateBase64String(8) }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = string.Empty,
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailUser()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = Guid.NewGuid().ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = string.Empty,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = RandomValues.CreateBase64String(8),
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            await _factory.UoW.UserRepo.UpdateAsync(user);

            rop = await _service.Raw.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.LegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client1 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var client2 = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            if (client1.Id == client2.Id)
                return;

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiDefaultRoleForUser)).Single();

            await _factory.UoW.UserRepo.AddToRoleAsync(user, role);
            await _factory.UoW.CommitAsync();

            /*
             */
            var empty_rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            empty_rop.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(empty_rop.access_token).Should().BeTrue();

            var empty_rop_claims = JwtHelper.ReadJwtToken(empty_rop.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            empty_rop_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            empty_rop_claims.Value.Split(':')[1].Should().Be(salt);

            var empty_rt = _service.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    grant_type = "refresh_token",
                    refresh_token = empty_rop.refresh_token,
                });
            empty_rt.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(empty_rt.access_token).Should().BeTrue();

            var empty_rt_claims = JwtHelper.ReadJwtToken(empty_rt.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            empty_rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            empty_rt_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            var single_rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Name }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            single_rop.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(single_rop.access_token).Should().BeTrue();

            var single_rop_claims = JwtHelper.ReadJwtToken(single_rop.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            single_rop_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            single_rop_claims.Value.Split(':')[1].Should().Be(salt);

            var single_rt = _service.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Name }),
                    grant_type = "refresh_token",
                    refresh_token = single_rop.refresh_token,
                });
            single_rt.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(single_rt.access_token).Should().BeTrue();

            var single_rt_claims = JwtHelper.ReadJwtToken(single_rt.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            single_rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            single_rt_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            var multiple_rop = _service.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Id.ToString(), client2.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            multiple_rop.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(multiple_rop.access_token).Should().BeTrue();

            var multiple_rop_claims = JwtHelper.ReadJwtToken(multiple_rop.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            multiple_rop_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            multiple_rop_claims.Value.Split(':')[1].Should().Be(salt);

            var multiple_rt = _service.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Id.ToString(), client2.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = multiple_rop.refresh_token,
                });
            multiple_rt.Should().BeAssignableTo<UserJwtV2>();

            JwtHelper.CanReadToken(multiple_rt.access_token).Should().BeTrue();

            var multiple_rt_claims = JwtHelper.ReadJwtToken(multiple_rt.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            multiple_rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            multiple_rt_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
