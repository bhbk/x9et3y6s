using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Helpers
{
    public class StsHelperV1
    {
        public async Task<HttpResponseMessage> GetAccessToken(TestServer owin, string client, string audience, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Statics.AttrClientIDV1, client),
                    new KeyValuePair<string, string>(Statics.AttrAudienceIDV1, audience),
                    new KeyValuePair<string, string>(Statics.AttrUserIDV1, user),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV1, "password")
                });
            return await owin.CreateClient().PostAsync("/oauth/v1/token", content);
        }

        public async Task<HttpResponseMessage> GetRefreshToken(TestServer owin, string client, string audience, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Statics.AttrClientIDV1, client),
                    new KeyValuePair<string, string>(Statics.AttrAudienceIDV1, audience),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                    new KeyValuePair<string, string>(Statics.AttrGrantTypeIDV1, "refresh_token")
                });
            return await owin.CreateClient().PostAsync("/oauth/v1/token", content);
        }
    }
}
