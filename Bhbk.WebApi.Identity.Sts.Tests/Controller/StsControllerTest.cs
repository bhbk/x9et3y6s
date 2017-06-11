using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Model;
using Bhbk.WebApi.Identity.Sts.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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
        public async Task Api_Sts_AccessToken_Auth_Fail_AudienceInvalid()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_AudienceNull()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), string.Empty, user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_ClientInvalid()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_ClientNull()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, string.Empty, audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserInvalid()
        {
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), string.Empty);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserLocked()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(60);
            BaseControllerTest.UoW.CustomUserManager.UpdateAsync(user).Wait();

            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserNull()
        {
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty, string.Empty);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Fail_UserPassword()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, EntrophyHelper.GenerateRandomBase64(8));

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];
            var valid = FormatHelper.ValidateJwtFormat(access);

            valid.Should().BeTrue();
        }

        [TestMethod]
        public void Api_Sts_AuthorizationCode_Use_Fail()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Api_Sts_AuthorizationCode_Use_Success()
        {
            Assert.Fail();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Revoke_Success()
        {
            var controller = new OAuthController(UoW);
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var token = user.Tokens.Where(x => x.ProtectedTicket == refresh).Single();
            var result = await controller.RevokeToken(token.Id) as OkResult;
            var check = await UoW.CustomUserManager.FindRefreshTokenByIdAsync(token.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_AudienceInvalid()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), EntrophyHelper.GenerateRandomBase64(8), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_AudienceNull()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), string.Empty, refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_ClientInvalid()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, EntrophyHelper.GenerateRandomBase64(8), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_ClientNull()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, string.Empty, audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_DateExpired()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();

            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshToken = true;
            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(-1);

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshToken = false;
            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_DateIssued()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();

            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshToken = true;
            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow.AddYears(1);

            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshToken = false;
            BaseControllerTest.UoW.CustomConfigManager.Config.UnitTestRefreshTokenFakeUtcNow = DateTime.UtcNow;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_TokenInvalid()
        {
            var random = new Random();
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, EntrophyHelper.GenerateRandomBase64(8)));

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_TokenNull()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), string.Empty);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_UserInvalid()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            await BaseControllerTest.UoW.CustomUserManager.DeleteAsync(user);

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Fail_UserLocked()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(60);
            await BaseControllerTest.UoW.CustomUserManager.UpdateAsync(user);

            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Use_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = await Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword);

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = await Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh);

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
