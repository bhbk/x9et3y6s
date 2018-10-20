using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Helpers;
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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class RefreshTokenProviderTest : StartupTest
    {
        private TestServer _owin;
        private S2STester _s2s;

        public RefreshTokenProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());

            _s2s = new S2STester(_conf, _ioc.ContextStatus, _owin);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_AudienceInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), RandomNumber.CreateBase64(8), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_AudienceMultiple()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audienceA.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_AudienceUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(RandomNumber.CreateBase64(8), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_ClientUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_DateExpired()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = true;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = false;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_DateIssued()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = true;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = false;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_TokenInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var random = new Random();
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, RandomNumber.CreateBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_TokenUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _ioc.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Fail_UserLocked()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

            var update = await _ioc.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Success_AudienceSingle_ById()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV1_Success_AudienceSingle_ByName()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV1(client.Name, audience.Name, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV1(client.Name, audience.Name, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_AudienceInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            audiences = new List<string> { RandomNumber.CreateBase64(8) };

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_ClientInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(RandomNumber.CreateBase64(8), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_ClientUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(string.Empty, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_DateExpired()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = true;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = false;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_DateIssued()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = true;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            _ioc.ConfigMgmt.Store.UnitTestsRefreshToken = false;
            _ioc.ConfigMgmt.Store.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_TokenInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var random = new Random();
            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh.Remove(pos, 8).Insert(pos, RandomNumber.CreateBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_TokenUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_UserInvalid()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await _ioc.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Fail_UserLocked()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await _ioc.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Success_AudienceMultiple_ById()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await _ioc.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Success_AudienceMultiple_ByName()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Name.ToString(), audienceB.Name.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _ioc.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await _ioc.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var access = await _s2s.StsAccessTokenV2(client.Name, audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Name, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Success_AudienceSingle_ById()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Success_AudienceSingle_ByName()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Name };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Name, audiences, user.Email, BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Name, audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshV2_Success_AudienceUndefined()
        {
            _data.Destroy();
            _data.CreateTest();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await _s2s.StsAccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestUserPassCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await _s2s.StsRefreshTokenV2(client.Id.ToString(), audiences, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
