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

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            audience.Enabled = false;
            IoC.AudienceMgmt.Store.Update(audience);

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientDisabled()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            client.Enabled = false;
            IoC.ClientMgmt.Store.Update(client);

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, string.Empty, audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            IoC.UserMgmt.UpdateAsync(user).Wait();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserPassword()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceSingle()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceMultiple()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audienceA = IoC.AudienceMgmt.Store.Get().First();
            var audienceB = IoC.AudienceMgmt.Store.Get().Last();
            var user = IoC.UserMgmt.Store.Get().First();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];

            var check = JwtV2Helper.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_AudienceInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateExpired()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            IoC.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            IoC.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            IoC.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            IoC.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateIssued()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            IoC.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            IoC.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            IoC.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            IoC.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var random = new Random();
            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserInvalid()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var delete = await IoC.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserLocked()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await IoC.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceMultiple()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audienceA = IoC.AudienceMgmt.Store.Get().First();
            var audienceB = IoC.AudienceMgmt.Store.Get().Last();
            var user = IoC.UserMgmt.Store.Get().First();

            if (audienceA.Id == audienceB.Id)
                Assert.Fail();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audienceA.Id.ToString() + "," + audienceB.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceSingle()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success_AudienceUndefined()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var client = IoC.ClientMgmt.Store.Get().First();
            var audience = IoC.AudienceMgmt.Store.Get().First();
            var user = IoC.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.StatusCode.Should().Be(HttpStatusCode.OK);

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
