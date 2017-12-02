using Bhbk.Lib.Identity.Helpers;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controllers
{
    [TestClass]
    public class StsControllerV2Test : StartupTest
    {
        private TestServer _owin;

        public StsControllerV2Test()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceDisabled()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            audience.Enabled = false;
            Context.AudienceMgmt.Store.Update(audience);

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_AudienceNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientDisabled()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            client.Enabled = false;
            Context.ClientMgmt.Store.Update(client);

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_ClientNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, string.Empty, audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserLocked()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);
            Context.UserMgmt.UpdateAsync(user).Wait();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Fail_UserPassword()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, EntrophyHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessTokenV2_Auth_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var result = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];

            var check = JwtHelperV2.IsValidJwtFormat(access);
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Revoke_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new OAuthController(Context);
            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var token = user.AppUserRefresh.Where(x => x.ProtectedTicket == refresh).Single();

            var result = await controller.RevokeToken(token.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_AudienceInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_AudienceNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_ClientNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateExpired()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            Context.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            Context.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            Context.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            Context.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_DateIssued()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            Context.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            Context.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            Context.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            Context.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var random = new Random();
            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, EntrophyHelper.GenerateRandomBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_TokenNull()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserInvalid()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var delete = await Context.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Fail_UserLocked()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

            var update = await Context.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshTokenV2_Use_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var client = Context.ClientMgmt.Store.Get().First();
            var audience = Context.AudienceMgmt.Store.Get().First();
            var user = Context.UserMgmt.Store.Get().First();

            var access = await StsV2.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await StsV2.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
