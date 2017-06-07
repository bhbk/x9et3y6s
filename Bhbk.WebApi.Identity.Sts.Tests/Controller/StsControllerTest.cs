using Bhbk.Lib.Identity.Helper;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task Api_Sts_AccessToken_Auth_Fail()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, EntrophyHelper.GenerateRandomBase64(8)).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];
            var valid = FormatHelper.ValidateJwtFormat(access);

            valid.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizationCode_Auth_Fail()
        {
            Assert.Fail();
        }

        [TestMethod]
        public async Task Api_Sts_AuthorizationCode_Auth_Success()
        {
            Assert.Fail();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Auth_Fail()
        {
            var random = new Random();
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);
            var result = Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, EntrophyHelper.GenerateRandomBase64(8))).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = Sts.GetAccessToken(_owin, client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = Sts.GetRefreshToken(_owin, client.Id.ToString(), audience.Id.ToString(), refresh).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
