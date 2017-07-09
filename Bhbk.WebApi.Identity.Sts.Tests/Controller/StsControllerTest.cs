using Bhbk.Lib.Identity.Helper;
using Bhbk.WebApi.Identity.Sts.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tests.Controller
{
    [TestClass]
    public class StsControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public StsControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_AudienceDisabled()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();

            audience.Enabled = false;
            BaseControllerTest.UoW.AudienceMgmt.Store.Update(audience);
            BaseControllerTest.UoW.AudienceMgmt.Store.Save();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_AudienceInvalid()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_AudienceNull()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_ClientDisabled()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();

            client.Enabled = false;
            BaseControllerTest.UoW.ClientMgmt.Store.Update(client);
            BaseControllerTest.UoW.ClientMgmt.Store.Save();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_ClientInvalid()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_ClientNull()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, string.Empty, audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserInvalid()
        {
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserLocked()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(60);
            BaseControllerTest.UoW.UserMgmt.UpdateAsync(user).Wait();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserNull()
        {
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty, string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserPassword()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, EntrophyHelper.GenerateRandomBase64(8));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];

            var check = FormatHelper.ValidateJwtFormat(access);
            check.Should().BeTrue();
        }

        //[TestMethod]
        //public void Api_Sts_AuthorizationCode_Use_Fail()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void Api_Sts_AuthorizationCode_Use_Success()
        //{
        //    Assert.Fail();
        //}

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Revoke_Success()
        {
            var controller = new OAuthController(UoW);
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var token = user.Tokens.Where(x => x.ProtectedTicket == refresh).Single();

            var result = await controller.RevokeToken(token.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            check.Should().BeAssignableTo(typeof(HttpResponseMessage));
            check.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_AudienceInvalid()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_AudienceNull()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_ClientInvalid()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_ClientNull()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, string.Empty, audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_DateExpired()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_DateIssued()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshToken = true;
            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshToken = false;
            BaseControllerTest.UoW.ConfigMgmt.Tweaks.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_TokenInvalid()
        {
            var random = new Random();
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, EntrophyHelper.GenerateRandomBase64(8)));
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_TokenNull()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_UserInvalid()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var delete = await BaseControllerTest.UoW.UserMgmt.DeleteAsync(user);
            delete.Should().BeAssignableTo(typeof(IdentityResult));
            delete.Succeeded.Should().BeTrue();

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_UserLocked()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(60);

            var update = await BaseControllerTest.UoW.UserMgmt.UpdateAsync(user);
            update.Should().BeAssignableTo(typeof(IdentityResult));
            update.Succeeded.Should().BeTrue();

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Success()
        {
            var user = BaseControllerTest.UoW.UserMgmt.Store.Get().First();
            var audience = BaseControllerTest.UoW.AudienceMgmt.Store.Get().First();
            var client = BaseControllerTest.UoW.ClientMgmt.Store.Get().First();

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestPasswordCurrent);
            access.Should().BeAssignableTo(typeof(HttpResponseMessage));
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);
            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
