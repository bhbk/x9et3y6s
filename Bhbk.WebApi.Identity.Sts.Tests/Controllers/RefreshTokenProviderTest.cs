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
        public async Task Api_Sts_OAuth_RefreshV1_Fail_AudienceDisabled()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            audience.Enabled = false;
            await _uow.AudienceRepo.UpdateAsync(audience);

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_AudienceNotFound()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            await _uow.AudienceRepo.DeleteAsync(audience);
            await _uow.CommitAsync();

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_AudienceUndefined()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_ClientDisabled()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            client.Enabled = false;
            await _uow.ClientRepo.UpdateAsync(client);

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_ClientNotFound()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            await _uow.ClientRepo.DeleteAsync(client);
            await _uow.CommitAsync();

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_ClientUndefined()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_DateExpired()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_DateIssued()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_TokenNotValid()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var random = new Random();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_TokenUndefined()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_UserNotFound()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _uow.CustomUserMgr.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Fail_UserLocked()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            var update = await _uow.CustomUserMgr.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Success_AudienceSingle_ById()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV1_Success_AudienceSingle_ByName()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV1(client.Name, audience.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV1(client.Name, audience.Name, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_AudienceDisabled()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            audience.Enabled = false;
            await _uow.AudienceRepo.UpdateAsync(audience);

            audiences = new List<string> { audience.Id.ToString() };

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_AudienceNotFound()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            audiences = new List<string> { RandomValues.CreateBase64String(8) };

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_ClientNotFound()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(RandomValues.CreateBase64String(8), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_ClientUndefined()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(string.Empty, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_DateExpired()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_DateIssued()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _uow.ConfigRepo.UnitTestsRefreshToken = true;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _uow.ConfigRepo.UnitTestsRefreshToken = false;
            _uow.ConfigRepo.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_TokenNotValid()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var random = new Random();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh.Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_TokenUndefined()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_UserNotValid()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _uow.CustomUserMgr.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Fail_UserLocked()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await _uow.CustomUserMgr.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Success_AudienceMultiple_ById()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audienceA = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audienceB = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _uow.CustomUserMgr.AddToRoleAsync(user, roleB.Name);

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Success_AudienceMultiple_ByName()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audienceA = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audienceB = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience2)).Single();
            var audiences = new List<string> { audienceA.Name.ToString(), audienceB.Name.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _uow.CustomRoleMgr.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _uow.CustomUserMgr.AddToRoleAsync(user, roleB.Name);

            var access = await _sts.AccessTokenV2(client.Name, audiences, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Name, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Success_AudienceSingle_ById()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Success_AudienceSingle_ByName()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { audience.Name };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Name, audiences, user.Email, Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Name, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_RefreshV2_Success_AudienceEmpty()
        {
            _uow.TestsDestroy();
            _uow.TestsCreate();

            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var audience = (await _uow.AudienceRepo.GetAsync(x => x.Name == Strings.ApiUnitTestAudience1)).Single();
            var audiences = new List<string> { string.Empty };
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var access = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _sts.RefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
