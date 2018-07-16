using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Helpers
{
    public class S2SClient : S2SHelper
    {
        public S2SClient(IConfigurationRoot conf, string context)
            : base(conf, context) { }
    }

    public class S2STester : S2SHelper
    {
        public S2STester(IConfigurationRoot conf, string context, TestServer connect)
            : base(conf, context, connect) { }
    }

    //https://oauth.com/playground/
    public class S2SHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public S2SHelper(IConfigurationRoot conf, string context)
        {
            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            if (!Enum.TryParse<ContextType>(context, out _context))
                throw new InvalidOperationException();

            _conf = conf;
            _connect = new HttpClient(connect);
        }

        public S2SHelper(IConfigurationRoot conf, string context, TestServer connect)
        {
            if (!Enum.TryParse<ContextType>(context, out _context))
                throw new InvalidOperationException();

            _conf = conf;
            _connect = connect.CreateClient();
        }

        public async Task<HttpResponseMessage> Admin_GetAudienceV1(JwtSecurityToken jwt, Guid audienceId)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return await _connect.GetAsync("/audience/v1/" + audienceId.ToString());
        }

        public async Task<HttpResponseMessage> Admin_GetClientV1(JwtSecurityToken jwt, Guid clientId)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return await _connect.GetAsync("/client/v1/" + clientId.ToString());
        }

        public async Task<HttpResponseMessage> Admin_GetRoleV1(JwtSecurityToken jwt, Guid roleId)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return await _connect.GetAsync("/role/v1/" + roleId.ToString());
        }

        public async Task<HttpResponseMessage> Admin_GetUserV1(JwtSecurityToken jwt, Guid userId)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return await _connect.GetAsync("/user/v1/" + userId.ToString());
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> Sts_AccessTokenV1(string client, string audience, string user, string secret)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", secret),
                });

            return await _connect.PostAsync("/oauth/v1/access", content);
        }

        public async Task<HttpResponseMessage> Sts_AccessTokenV2(string client, List<string> audiences, string user, string secret)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("user", user),
                    new KeyValuePair<string, string>("password", secret),
                });

            return await _connect.PostAsync("/oauth/v2/access", content);
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> Sts_AuthorizationCodeRequestV1(string client, string audience, string user, string redirectUri, string scope)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            return await _connect.GetAsync("/oauth/v1/authorization-code" + content);
        }

        public async Task<HttpResponseMessage> Sts_AuthorizationCodeRequestV2(string client, string audience, string user, string redirectUri, string scope)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            return await _connect.GetAsync("/oauth/v2/authorization-code" + content);
        }

        public async Task<HttpResponseMessage> Sts_AuthorizationCodeV1(string client, string user, string redirectUri, string code)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            return await _connect.GetAsync("/oauth/v1/authorization" + content);
        }

        public async Task<HttpResponseMessage> Sts_AuthorizationCodeV2(string client, string user, string redirectUri, string code)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            return await _connect.GetAsync("/oauth/v2/authorization" + content);
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> Sts_ClientCredentialsV1(string client, string secret)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            return await _connect.PostAsync("/oauth/v1/client", content);
        }

        public async Task<HttpResponseMessage> Sts_ClientCredentialsV2(string client, string secret)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            return await _connect.PostAsync("/oauth/v2/client", content);
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> Sts_RefreshTokenV1(string client, string audience, string refresh)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            return await _connect.PostAsync("/oauth/v1/refresh", content);
        }

        public async Task<HttpResponseMessage> Sts_RefreshTokenV2(string client, List<string> audiences, string refresh)
        {
            if (_context == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            return await _connect.PostAsync("/oauth/v2/refresh", content);
        }
    }
}
