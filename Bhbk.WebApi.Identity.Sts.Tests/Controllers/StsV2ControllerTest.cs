using Bhbk.Lib.Identity.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class StsV2ControllerTest : StartupTest
    {
        private TestServer _owin;

        public StsV2ControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceDisabled()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            audience.Enabled = false;
            TestIoC.AudienceMgmt.Store.Update(audience);

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceMultiple()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientDisabled()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            client.Enabled = false;
            TestIoC.ClientMgmt.Store.Update(client);

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, string.Empty, audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            TestIoC.UserMgmt.UpdateAsync(user).Wait();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserPassword()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceSingle_ById()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceSingle_ByName()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Name, audience.Name, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceMultiple_ById()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceMultiple_ByName()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var result = await TestStsV2.GetAccessToken(_owin, client.Name, audienceA.Name + "," + audienceB.Name, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var result = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await result.Content.ReadAsStringAsync());
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateExpired()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshToken = true;
            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshToken = false;
            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateIssued()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshToken = true;
            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshToken = false;
            TestIoC.ConfigMgmt.Tweaks.UnitTestsRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var random = new Random();
            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var delete = await TestIoC.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await TestIoC.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceMultiple_ById()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceMultiple_ByName()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audienceA = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var audienceB = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceB).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var roleB = TestIoC.RoleMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestRoleB).Single();
            await TestIoC.UserMgmt.AddToRoleAsync(user, roleB.Name);

            var access = await TestStsV2.GetAccessToken(_owin, client.Name, audienceA.Name + "," + audienceB.Name, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Name, audienceA.Name + "," + audienceB.Name, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceSingle_ById()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceSingle_ByName()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Name, audience.Name, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Name, audience.Name, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            var access = await TestStsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Id.ToString(), BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(await access.Content.ReadAsStringAsync());
            var refresh = (string)jwt["refresh_token"];

            var result = await TestStsV2.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
