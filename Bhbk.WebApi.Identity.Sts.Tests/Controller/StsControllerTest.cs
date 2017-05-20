using Bhbk.Lib.Identity.Helper;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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
            var result = GetAccessToken(client.Id.ToString(), audience.Id.ToString(), user.Email, EntrophyHelper.GenerateRandomBase64(8)).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_AccessToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var result = GetAccessToken(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            var access = (string)jwt["access_token"];
            var valid = FormatHelper.ValidateJwtFormat(access);

            valid.Should().BeTrue();
        }

        //[TestMethod]
        //public async Task Api_Sts_AuthorizationCode_Auth_Fail()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public async Task Api_Sts_AuthorizationCode_Auth_Success()
        //{
        //    var user = BaseControllerTest.UoW.UserRepository.Get().First();
        //    var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
        //    var result = GetAuthorizationRequest(audience.Id.ToString(), "https://localhost:44304/api/identity/sts/oauth/v1/token", "annoyed").Result;

        //    result.Should().NotBeNull();
        //    result.IsSuccessStatusCode.Should().BeTrue();
        //}

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Auth_Fail()
        {
            var random = new Random();
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = GetAccessToken(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var pos = random.Next(refresh.Length - 8);
            var result = GetRefreshToken(client.Id.ToString(), audience.Id.ToString(), refresh.Remove(pos, 8).Insert(pos, EntrophyHelper.GenerateRandomBase64(8))).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Sts_RefreshToken_Auth_Success()
        {
            var user = BaseControllerTest.UoW.UserRepository.Get().First();
            var audience = BaseControllerTest.UoW.AudienceRepository.Get().First();
            var client = BaseControllerTest.UoW.ClientRepository.Get().First();
            var access = GetAccessToken(client.Id.ToString(), audience.Id.ToString(), user.Email, BaseLib.Statics.ApiUnitTestsPassword).Result;

            access.Should().NotBeNull();
            access.IsSuccessStatusCode.Should().BeTrue();

            var jwt = JObject.Parse(access.Content.ReadAsStringAsync().Result);
            var refresh = (string)jwt["refresh_token"];
            var result = GetRefreshToken(client.Id.ToString(), audience.Id.ToString(), refresh).Result;

            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        private async Task<HttpResponseMessage> GetAuthorizationRequest(string clientID, string redirectUri, string state)
        {
            //var content = new FormUrlEncodedContent(new[]
            //    {
            //        new KeyValuePair<string, string>("client_id", clientID),
            //        new KeyValuePair<string, string>("redirect_uri", redirectUri),
            //        new KeyValuePair<string, string>("state", state),
            //        new KeyValuePair<string, string>("scope", "all"),
            //        new KeyValuePair<string, string>("response_type", "code"),
            //    });

            string content = HttpUtility.UrlPathEncode("?client_id=" + clientID
                + "&redirect_uri=" + redirectUri
                + "&state=" + state
                + "&scope=" + "all"
                + "&response_type=" + "code");
            
            return _owin.HttpClient.GetAsync("/oauth/v1/authorize" + content).Result;
        }

        private async Task<HttpResponseMessage> GetAuthorizationCode(string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                });
            return _owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        private async Task<HttpResponseMessage> GetAccessToken(string clientID, string audienceID, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("audience_id", audienceID),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password")
                });
            return _owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        private async Task<HttpResponseMessage> GetClientCredentials(string clientID, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });
            return _owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        private async Task<HttpResponseMessage> GetRefreshToken(string clientID, string audienceID, string token)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("audience_id", audienceID),
                    new KeyValuePair<string, string>("refresh_token", token),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });
            return _owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }
    }
}
