using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Helpers
{
    public class StsHelperV2
    {
        public async Task<HttpResponseMessage> GetAuthorizationRequest(TestServer owin, string client, string redirectUri, string state)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&redirect_uri=" + redirectUri
                + "&state=" + state
                + "&scope=" + "all"
                + "&response_type=" + "code");

            return await owin.CreateClient().GetAsync("/oauth/v2/authorize" + content);
        }

        public async Task<HttpResponseMessage> GetAuthorizationCode(TestServer owin, string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV2, "authorization_code"),
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/authorize", content);
        }

        public async Task<HttpResponseMessage> GetAccessToken(TestServer owin, string clients, string audiences, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Statics.AttrClientIDV2, clients),
                    new KeyValuePair<string, string>(Statics.AttrAudienceIDV2, audiences),
                    new KeyValuePair<string, string>(Statics.AttrUserIDV2, user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV2, "password")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/access", content);
        }

        public async Task<HttpResponseMessage> GetClientCredentials(TestServer owin, string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Statics.AttrClientIDV2, client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV2, "client_credentials")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/client", content);
        }

        public async Task<HttpResponseMessage> GetRefreshToken(TestServer owin, string clients, string audiences, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Statics.AttrClientIDV2, clients),
                    new KeyValuePair<string, string>(Statics.AttrAudienceIDV2, audiences),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV2, "refresh_token")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/refresh", content);
        }
    }
}
