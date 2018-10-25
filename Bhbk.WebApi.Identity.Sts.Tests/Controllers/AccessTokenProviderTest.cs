﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interop;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
        public async Task Api_Sts_OAuth_AccessV1_Fail_AudienceDisabled()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            audience.Enabled = false;
            _ioc.AudienceMgmt.Store.Update(audience);

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_AudienceNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = RandomValues.CreateBase64String(8);
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_AudienceUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = string.Empty;
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_ClientDisabled()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            client.Enabled = false;
            _ioc.ClientMgmt.Store.Update(client);

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_ClientNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = RandomValues.CreateBase64String(8);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(client, audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_ClientUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = string.Empty;
            var audience = _ioc.AudienceMgmt.Store.Get().First();
            var user = _ioc.UserMgmt.Store.Get().First();

            var result = await _sts.AccessTokenV1(client, audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_CompatibiltyMode_Issuer()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _ioc.ConfigStore.Values.DefaultsCompatibilityModeIssuer = false;

            var result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_UserNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_UserLocked()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);
            _ioc.UserMgmt.UpdateAsync(user).Wait();

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_UserUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = string.Empty;

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Fail_UserPassword()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var password = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), password);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Success_Audience_ById()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Success_Audience_ByName()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV1(client.Name, audience.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();

            result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Name, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            access = (string)jwt["access_token"];

            check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV1_Success_CompatibiltyMode_Issuer()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            _ioc.ConfigStore.Values.DefaultsCompatibilityModeIssuer = true;

            var result = await _sts.AccessTokenV1CompatibilityModeIssuer(audience.Id.ToString(), user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_AudienceDisabled_Single()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            audienceA.Enabled = false;
            _ioc.AudienceMgmt.Store.Update(audienceA);

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_AudienceDisabled_Multiple()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            audienceA.Enabled = true;
            audienceB.Enabled = false;
            _ioc.AudienceMgmt.Store.Update(audienceA);

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_AudienceNotFound_Multiple()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString(), RandomValues.CreateBase64String(8) };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_AudienceNotFound_Single()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audiences = new List<string> { RandomValues.CreateBase64String(8) };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_ClientDisabled()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            client.Enabled = false;
            _ioc.ClientMgmt.Store.Update(client);

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_ClientNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = RandomValues.CreateBase64String(8);
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client, audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_ClientUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = string.Empty;
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client, audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_UserLocked()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            _ioc.UserMgmt.UpdateAsync(user).Wait();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_UserNotFound()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = RandomValues.CreateBase64String(8);

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_UserUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, string.Empty, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Fail_UserPassword()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), RandomValues.CreateBase64String(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Success_AudienceSingle_ById()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Success_AudienceSingle_ByName()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audience = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audiences = new List<string> { audience.Name };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Name, audiences, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Success_AudienceMultiple_ById()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _ioc.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Success_AudienceMultiple_ByName()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audienceA = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience1).Single();
            var audienceB = _ioc.AudienceMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestAudience2).Single();
            var audiences = new List<string> { audienceA.Name.ToString(), audienceB.Name.ToString() };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = _ioc.RoleMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestRole2).Single();
            await _ioc.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await _sts.AccessTokenV2(client.Name, audiences, user.Email, Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_OAuth_AccessV2_Success_AudienceUndefined()
        {
            _tests.DestroyAll();
            _tests.Create();

            var client = _ioc.ClientMgmt.Store.Get(x => x.Name == Strings.ApiUnitTestClient1).Single();
            var audiences = new List<string> { string.Empty };
            var user = _ioc.UserMgmt.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            var result = await _sts.AccessTokenV2(client.Id.ToString(), audiences, user.Id.ToString(), Strings.ApiUnitTestUserPassCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtSecureProvider.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }
    }
}
