﻿using Bhbk.Lib.Core.Cryptography;
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
    public class ClientCredentialsServiceTests
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _client;
        private readonly IStsService _service;

        public ClientCredentialsServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new StsService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Refresh_NotImplemented()
        {
            var rt = await _service.Raw.ClientCredentialRefresh_UseV1(
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

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV1_Use_NotImplemented()
        {
            var cc = await _service.Raw.ClientCredential_UseV1(
                new ClientCredentialV1()
                {
                    issuer_id = Guid.NewGuid().ToString(),
                    client_id = Guid.NewGuid().ToString(),
                    client_secret = RandomValues.CreateAlphaNumericString(8),
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });

            var rt = await _service.Raw.ClientCredentialRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            rt = await _service.Raw.ClientCredentialRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });

            var rt = await _service.Raw.ClientCredentialRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rt = await _service.Raw.ClientCredentialRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = string.Empty,
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
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
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Refresh_FailToken()
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

            var cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            var rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = true;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });

            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFake = false;
            _factory.UoW.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            /*
             */
            cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });

            var cc_pos = random.Next((cc.refresh_token).Length - 8);

            rt = await _service.Raw.ResourceOwnerRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client.Id.ToString() }),
                    grant_type = "refresh_token",
                    refresh_token = (cc.refresh_token).Remove(cc_pos, 8).Insert(cc_pos, RandomValues.CreateBase64String(8)),
                });
            rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

            /*
             */
            cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
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
        public async Task Sts_OAuth2_ClientCredentialV2_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var cc = await _service.Raw.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = Guid.NewGuid().ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            cc = await _service.Raw.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = RandomValues.CreateBase64String(16),
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            client.Enabled = false;

            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            cc = await _service.Raw.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Use_FailIssuer()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var cc = await _service.Raw.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = Guid.NewGuid().ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.NotFound);

            issuer.Enabled = false;

            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            await _factory.UoW.CommitAsync();

            cc = await _service.Raw.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo(typeof(HttpResponseMessage));
            cc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_ClientCredentialV2_Use_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var salt = _factory.Conf["IdentityTenants:Salt"];
            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var cc = _service.ClientCredential_UseV2(
                new ClientCredentialV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    client_secret = client.ClientKey,
                });
            cc.Should().BeAssignableTo<ClientJwtV2>();

            JwtHelper.CanReadToken(cc.access_token).Should().BeTrue();

            var cc_claims = JwtHelper.ReadJwtToken(cc.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            cc_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            cc_claims.Value.Split(':')[1].Should().Be(salt);

            var rt = _service.ClientCredentialRefresh_UseV2(
                new RefreshTokenV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = client.Id.ToString(),
                    grant_type = "refresh_token",
                    refresh_token = cc.refresh_token,
                });
            rt.Should().BeAssignableTo<ClientJwtV2>();

            JwtHelper.CanReadToken(rt.access_token).Should().BeTrue();

            var rt_claims = JwtHelper.ReadJwtToken(rt.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rt_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            rt_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
