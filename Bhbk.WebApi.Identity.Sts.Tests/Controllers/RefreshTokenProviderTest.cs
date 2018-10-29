using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Primitives;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class RefreshTokenProviderTest : StartupTest
    {
        private TestServer _owin;
        private StsTester _sts;

        public RefreshTokenProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _sts = new StsTester(_conf, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_ClientDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            client.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(client);

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_ClientNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            await _uow.ClientRepo.DeleteAsync(client);
            await _uow.CommitAsync();

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_ClientUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_IssuerDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            issuer.Enabled = false;
            await _uow.IssuerRepo.UpdateAsync(issuer);

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            await _uow.IssuerRepo.DeleteAsync(issuer);
            await _uow.CommitAsync();

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_IssuerUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(string.Empty, client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_DateExpired()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_DateIssued()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_TokenNotValid()
        {
            _tests.Destroy();
            _tests.Create();

            var random = new Random();
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_TokenUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_UserNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _uow.CustomUserMgr.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Fail_UserLocked()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            var update = await _uow.CustomUserMgr.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Success_ClientSingle_ById()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Id.ToString(), client.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Id.ToString(), client.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV1_Success_ClientSingle_ByName()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(issuer.Name, client.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(issuer.Name, client.Name, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_ClientDisabled()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            client.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(client);

            clients = new List<string> { client.Id.ToString() };

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_ClientNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            clients = new List<string> { RandomValues.CreateBase64String(8) };

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_IssuerNotFound()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(RandomValues.CreateBase64String(8), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_IssuerUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(string.Empty, clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_DateExpired()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_DateIssued()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_TokenNotValid()
        {
            _tests.Destroy();
            _tests.Create();

            var random = new Random();
            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh.Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_TokenUndefined()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_UserNotValid()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _uow.CustomUserMgr.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Fail_UserLocked()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await _uow.CustomUserMgr.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Success_ClientMultiple_ById()
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

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Success_ClientMultiple_ByName()
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

            var access = await _sts.AccessTokenV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Name, clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Success_ClientSingle_ById()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Success_ClientSingle_ByName()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { client.Name };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Name, clients, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Name, clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth2_RefreshV2_Success_ClientEmpty()
        {
            _tests.Destroy();
            _tests.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var clients = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(issuer.Id.ToString(), clients, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(issuer.Id.ToString(), clients, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
