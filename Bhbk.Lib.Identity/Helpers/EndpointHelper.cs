using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Lib.Identity.Helpers
{
    public class EndpointHelper
    {
        public async Task<HttpResponseMessage> GetAuthorizationRequestV2(TestServer owin, string client, string redirectUri, string state)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&redirect_uri=" + redirectUri
                + "&state=" + state
                + "&scope=" + "all"
                + "&response_type=" + "code");

            return await owin.CreateClient().GetAsync("/oauth/v2/authorize" + content);
        }

        public async Task<HttpResponseMessage> GetAuthorizationCodeV2(TestServer owin, string code, string redirectUri)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV2, "authorization_code"),
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/authorize", content);
        }

        public async Task<HttpResponseMessage> GetAccessTokenV1(TestServer owin, string client, string audience, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrClientIDV1, client),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrAudienceIDV1, audience),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrUserIDV1, user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV1, "password")
                });
            return await owin.CreateClient().PostAsync("/oauth/v1/access", content);
        }

        public async Task<HttpResponseMessage> GetAccessTokenV2(TestServer owin, string client, List<string> audiences, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrClientIDV2, client),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrAudienceIDV2, string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrUserIDV2, user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV2, "password")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/access", content);
        }

        public async Task<HttpResponseMessage> GetClientCredentialsV2(TestServer owin, string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrClientIDV2, client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV2, "client_credentials")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/client", content);
        }

        public async Task<HttpResponseMessage> GetRefreshTokenV1(TestServer owin, string client, string audience, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrClientIDV1, client),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrAudienceIDV1, audience),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV1, "refresh_token")
                });
            return await owin.CreateClient().PostAsync("/oauth/v1/refresh", content);
        }

        public async Task<HttpResponseMessage> GetRefreshTokenV2(TestServer owin, string client, List<string> audiences, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrClientIDV2, client),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrAudienceIDV2,string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                    new KeyValuePair<string, string>(BaseLib.Statics.AttrGrantTypeIDV2, "refresh_token")
                });
            return await owin.CreateClient().PostAsync("/oauth/v2/refresh", content);
        }
    }
}
