using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
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
    public class ResourceOwnerServiceTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public ResourceOwnerServiceTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = true;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = Guid.NewGuid().ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _endpoints.ResourceOwner_UseV1Legacy(
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

            rop = await _endpoints.ResourceOwner_UseV1Legacy(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            rop = await _endpoints.ResourceOwner_UseV1(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = true;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = Guid.NewGuid().ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

            rop = await _endpoints.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = string.Empty,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            rop = await _endpoints.ResourceOwner_UseV1Legacy(
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

            rop = await _endpoints.ResourceOwner_UseV1Legacy(
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
            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var rop = await _endpoints.ResourceOwner_UseV1(
                new ResourceOwnerV1()
                {
                    issuer_id = issuer.Id.ToString(),
                    client_id = client.Id.ToString(),
                    username = user.Email,
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
            rop.StatusCode.Should().Be(HttpStatusCode.OK);

            var rop_ok = JObject.Parse(await rop.Content.ReadAsStringAsync());
            var rop_check = rop_ok.ToObject<UserJwtV1>();
            rop_check.Should().BeAssignableTo<UserJwtV1>();

            JwtBuilder.CanReadToken(rop_check.access_token).Should().BeTrue();

            var rop_claims = JwtBuilder.ReadJwtToken(rop_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            rop_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            rop_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = true;

            var legacy = await _endpoints.ResourceOwner_UseV1Legacy(
                new ResourceOwnerV1()
                {
                    client_id = client.Id.ToString(),
                    username = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            legacy.Should().BeAssignableTo(typeof(HttpResponseMessage));
            legacy.StatusCode.Should().Be(HttpStatusCode.OK);

            var legacy_ok = JObject.Parse(await legacy.Content.ReadAsStringAsync());
            var legacy_check = legacy_ok.ToObject<UserJwtV1Legacy>();
            legacy_check.Should().BeAssignableTo<UserJwtV1Legacy>();

            JwtBuilder.CanReadToken(legacy_check.access_token).Should().BeTrue();

            var legacy_claims = JwtBuilder.ReadJwtToken(legacy_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            legacy_claims.Value.Should().Be(Strings.ApiUnitTestIssuer);
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailClient()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            rop = await _endpoints.ResourceOwner_UseV2(
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

            _factory.UoW.ConfigRepo.DefaultsLegacyModeIssuer = false;

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
            var empty = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { string.Empty }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            empty.Should().BeAssignableTo(typeof(HttpResponseMessage));
            empty.StatusCode.Should().Be(HttpStatusCode.OK);

            var empty_ok = JObject.Parse(await empty.Content.ReadAsStringAsync());
            var empty_check = empty_ok.ToObject<UserJwtV2>();
            empty_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(empty_check.access_token).Should().BeTrue();

            var empty_claims = JwtBuilder.ReadJwtToken(empty_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            empty_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            empty_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            var single = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Name }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            single.Should().BeAssignableTo(typeof(HttpResponseMessage));
            single.StatusCode.Should().Be(HttpStatusCode.OK);

            var single_ok = JObject.Parse(await single.Content.ReadAsStringAsync());
            var single_check = single_ok.ToObject<UserJwtV2>();
            single_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(single_check.access_token).Should().BeTrue();

            var single_claims = JwtBuilder.ReadJwtToken(single_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            single_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            single_claims.Value.Split(':')[1].Should().Be(salt);

            /*
             */
            var multiple = await _endpoints.ResourceOwner_UseV2(
                new ResourceOwnerV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = string.Join(",", new List<string> { client1.Id.ToString(), client2.Id.ToString() }),
                    user = user.Id.ToString(),
                    grant_type = "password",
                    password = Strings.ApiUnitTestUserPassCurrent,
                });
            multiple.Should().BeAssignableTo(typeof(HttpResponseMessage));
            multiple.StatusCode.Should().Be(HttpStatusCode.OK);

            var multiple_ok = JObject.Parse(await multiple.Content.ReadAsStringAsync());
            var multiple_check = multiple_ok.ToObject<UserJwtV2>();
            multiple_check.Should().BeAssignableTo<UserJwtV2>();

            JwtBuilder.CanReadToken(multiple_check.access_token).Should().BeTrue();

            var multiple_claims = JwtBuilder.ReadJwtToken(multiple_check.access_token).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            multiple_claims.Value.Split(':')[0].Should().Be(Strings.ApiUnitTestIssuer);
            multiple_claims.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
