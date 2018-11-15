using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Primitives;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [Collection("StsTestCollection")]
    public class AccessTokenProviderTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly StsClient _endpoints;

        public AccessTokenProviderTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new StsClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_ClientDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            client.Enabled = false;
            await _factory.UoW.ClientRepo.UpdateAsync(client);
            await _factory.UoW.CommitAsync();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_ClientNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = RandomValues.CreateBase64String(8);
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_ClientUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = string.Empty;
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_IssuerDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync()).First();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            issuer.Enabled = false;
            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = RandomValues.CreateBase64String(8);
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer, client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_IssuerUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = string.Empty;
            var client = (await _factory.UoW.ClientRepo.GetAsync()).First();
            var user = (await _factory.UoW.UserMgr.GetAsync()).First();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer, client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = RandomValues.CreateBase64String(8);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_UserLocked()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);
            await _factory.UoW.UserMgr.UpdateAsync(user);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_UserUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = string.Empty;

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Fail_UserPassword()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var password = RandomValues.CreateBase64String(8);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Success_Client_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Success_Client_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Name, client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            result = await _endpoints.AccessToken_GenerateV1_CompatibilityModeIssuer(client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV1_Success_IssuerSalt()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            _factory.UoW.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _endpoints.AccessToken_GenerateV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var salt = _factory.Conf["IdentityTenants:Salt"];

            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtSecureProvider.ReadJwtToken(access).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();

            check.Value.Split(':')[0].Should().Be(issuer.Name);
            check.Value.Split(':')[1].Should().Be(salt);

            check = JwtSecureProvider.ReadJwtToken(refresh).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();

            check.Value.Split(':')[0].Should().Be(issuer.Name);
            check.Value.Split(':')[1].Should().Be(salt);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_ClientDisabled_Multiple()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            if (clientA.Id == clientB.Id)
                return;

            clientA.Enabled = true;
            clientB.Enabled = false;
            await _factory.UoW.ClientRepo.UpdateAsync(clientA);
            await _factory.UoW.ClientRepo.UpdateAsync(clientB);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_ClientDisabled_Single()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            clientA.Enabled = false;
            await _factory.UoW.ClientRepo.UpdateAsync(clientA);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_ClientNotFound_Multiple()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString(), RandomValues.CreateBase64String(8) };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_ClientNotFound_Single()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { RandomValues.CreateBase64String(8) };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_IssuerDisabled()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            issuer.Enabled = false;
            await _factory.UoW.IssuerRepo.UpdateAsync(issuer);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_IssuerNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = RandomValues.CreateBase64String(8);
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer, clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_IssuerUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = string.Empty;
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer, clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_UserLocked()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            _factory.UoW.UserMgr.UpdateAsync(user).Wait();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_UserNotFound()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = RandomValues.CreateBase64String(8);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_UserUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, string.Empty, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Fail_UserPassword()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), RandomValues.CreateBase64String(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_ClientSingle_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_ClientSingle_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Name };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_ClientMultiple_ById()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            if (clientA.Id == clientB.Id)
                return;

            var role = (await _factory.UoW.RoleMgr.GetAsync(x => x.Name == Strings.ApiUnitTestRole2)).Single();
            await _factory.UoW.UserMgr.AddToRoleAsync(user, role.Name);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_ClientMultiple_ByName()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Name.ToString(), clientB.Name.ToString() };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            if (clientA.Id == clientB.Id)
                return;

            var role = (await _factory.UoW.RoleMgr.GetAsync(x => x.Name == Strings.ApiUnitTestRole2)).Single();
            await _factory.UoW.UserMgr.AddToRoleAsync(user, role.Name);

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_ClientUndefined()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [Fact]
        public async Task Sts_OAuth2_AccessV2_Success_IssuerSalt()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var result = await _endpoints.AccessToken_GenerateV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var salt = _factory.Conf["IdentityTenants:Salt"];

            salt.Should().Be(_factory.UoW.IssuerRepo.Salt);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];
            var refresh = (string)jwt["refresh_token"];

            var check = JwtSecureProvider.ReadJwtToken(access).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();

            check.Value.Split(':')[0].Should().Be(issuer.Name);
            check.Value.Split(':')[1].Should().Be(salt);

            check = JwtSecureProvider.ReadJwtToken(refresh).Claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();

            check.Value.Split(':')[0].Should().Be(issuer.Name);
            check.Value.Split(':')[1].Should().Be(salt);
        }
    }
}
