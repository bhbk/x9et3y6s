using Bhbk.Lib.Identity.Helpers;
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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class AccessTokenProviderTest : StartupTest
    {
        private TestServer _owin;

        public AccessTokenProviderTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_AudienceDisabled()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            audience.Enabled = false;
            TestIoC.AudienceMgmt.Store.Update(audience);

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8);
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_AudienceMultiple()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), string.Join(",", audiences.Select(x => x)), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = string.Empty;
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_ClientDisabled()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            client.Enabled = false;
            TestIoC.ClientMgmt.Store.Update(client);

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client, audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = string.Empty;
            var audience = TestIoC.AudienceMgmt.Store.Get().First();
            var user = TestIoC.UserMgmt.Store.Get().First();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client, audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8);

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            TestIoC.UserMgmt.UpdateAsync(user).Wait();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_UserUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = string.Empty;

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Fail_UserPassword()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Pass_AudienceSingle_ById()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV1_Pass_AudienceSingle_ByName()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV1(_owin, client.Name, audience.Name, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_AudienceDisabled()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            audience.Enabled = false;
            TestIoC.AudienceMgmt.Store.Update(audience);

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audiences = new List<string> { BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8) };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_AudienceMultiple()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_ClientDisabled()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            client.Enabled = false;
            TestIoC.ClientMgmt.Store.Update(client);

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client, audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = string.Empty;
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client, audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            TestIoC.UserMgmt.UpdateAsync(user).Wait();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_UserUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, string.Empty, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Fail_UserPassword()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Pass_AudienceSingle_ById()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Pass_AudienceSingle_ByName()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audiences = new List<string> { audience.Name };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Name, audiences, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Pass_AudienceMultiple_ById()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Id.ToString(), audienceB.Id.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Pass_AudienceMultiple_ByName()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var audiences = new List<string> { audienceA.Name.ToString(), audienceB.Name.ToString() };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Name, audiences, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_StsProvider_AccessV2_Pass_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTest();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audiences = new List<string> { string.Empty };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestEndpoints.GetAccessTokenV2(_owin, client.Id.ToString(), audiences, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtHelper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }
    }
}
