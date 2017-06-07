using Microsoft.Owin.Testing;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Helper
{
    public class TestingHelper
    {
        public async Task<HttpResponseMessage> GetAuthorizationRequest(TestServer owin, string clientID, string redirectUri, string state)
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

            return owin.HttpClient.GetAsync("/oauth/v1/authorize" + content).Result;
        }

        public async Task<HttpResponseMessage> GetAuthorizationCode(TestServer owin, string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                });
            return owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        public async Task<HttpResponseMessage> GetAccessToken(TestServer owin, string clientID, string audienceID, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("audience_id", audienceID),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password")
                });
            return owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        public async Task<HttpResponseMessage> GetClientCredentials(TestServer owin, string clientID, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });
            return owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }

        public async Task<HttpResponseMessage> GetRefreshToken(TestServer owin, string clientID, string audienceID, string token)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("audience_id", audienceID),
                    new KeyValuePair<string, string>("refresh_token", token),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });
            return owin.HttpClient.PostAsync("/oauth/v1/token", content).Result;
        }
    }
}
