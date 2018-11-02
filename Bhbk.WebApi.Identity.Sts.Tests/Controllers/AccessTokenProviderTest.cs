using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class AccessTokenProviderTest : StartupTest
    {
        private TestServer _owin;
        private StsTester _sts;

        public AccessTokenProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _sts = new StsTester(_conf, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_ClientDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            client.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(client);
            await _uow.CommitAsync();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_ClientNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = RandomValues.CreateBase64String(8);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_ClientUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = string.Empty;
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_IssuerDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            issuer.Enabled = false;
            await _uow.IssuerRepo.UpdateAsync(issuer);

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = RandomValues.CreateBase64String(8);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer, client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_IssuerUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = string.Empty;
            var client = (await _uow.ClientRepo.GetAsync()).First();
            var user = _uow.CustomUserMgr.Store.Get().First();

            var result = await _sts.AccessTokenV1(issuer, client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_IssuerCompatibilty()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.DefaultsCompatibilityModeIssuer = false;

            var result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_UserNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_UserLocked()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);
            _uow.CustomUserMgr.UpdateAsync(user).Wait();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_UserUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = string.Empty;

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Fail_UserPassword()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var password = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Success_Client_ById()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Success_Client_ByName()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer.Name, client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Success_IssuerCompatibilty()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.DefaultsCompatibilityModeIssuer = true;

            var result = await _sts.AccessTokenV1CompatibilityModeIssuer(client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV1_Success_IssuerSalt()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var salt = _conf["IdentityTenants:Salt"];

            salt.Should().Be(_uow.IssuerRepo.Salt);

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

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_ClientDisabled_Multiple()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (clientA.Id == clientB.Id)
                Assert.Fail();

            clientA.Enabled = true;
            clientB.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(clientA);
            await _uow.ClientRepo.UpdateAsync(clientB);

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_ClientDisabled_Single()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            clientA.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(clientA);

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_ClientNotFound_Multiple()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString(), RandomValues.CreateBase64String(8) };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_ClientNotFound_Single()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { RandomValues.CreateBase64String(8) };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_IssuerDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            issuer.Enabled = false;
            await _uow.IssuerRepo.UpdateAsync(issuer);

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = RandomValues.CreateBase64String(8);
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer, clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_IssuerUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = string.Empty;
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer, clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_UserLocked()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            _uow.CustomUserMgr.UpdateAsync(user).Wait();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_UserNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_UserUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, string.Empty, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Fail_UserPassword()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), RandomValues.CreateBase64String(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_ClientSingle_ById()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_ClientSingle_ByName()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Name };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_ClientMultiple_ById()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Id.ToString(), clientB.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (clientA.Id == clientB.Id)
                Assert.Fail();

            var roleB = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _uow.CustomUserMgr.AddToRoleAsync(user, roleB.Name);

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_ClientMultiple_ByName()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clientA = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clientB = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient2)).Single();
            var clients = new List<string> { clientA.Name.ToString(), clientB.Name.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (clientA.Id == clientB.Id)
                Assert.Fail();

            var roleB = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _uow.CustomUserMgr.AddToRoleAsync(user, roleB.Name);

            var result = await _sts.AccessTokenV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_ClientUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.CanReadToken(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_AccessV2_Success_IssuerSalt()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var salt = _conf["IdentityTenants:Salt"];

            salt.Should().Be(_uow.IssuerRepo.Salt);

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
