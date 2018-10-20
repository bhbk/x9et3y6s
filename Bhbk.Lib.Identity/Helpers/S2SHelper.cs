using Bhbk.Lib.Primitives.Enums;
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
        public S2SClient(IConfigurationRoot conf, ContextType context)
            : base(conf, context) { }
    }

    public class S2STester : S2SHelper
    {
        public S2STester(IConfigurationRoot conf, ContextType context, TestServer connect)
            : base(conf, context, connect) { }
    }

    //https://oauth.com/playground/
    public class S2SHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public S2SHelper(IConfigurationRoot conf, ContextType context)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _context = context;
            _conf = conf;
            _connect = new HttpClient(connect);
        }

        public S2SHelper(IConfigurationRoot conf, ContextType context, TestServer connect)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _context = context;
            _conf = conf;
            _connect = connect.CreateClient();
        }

        public async Task<HttpResponseMessage> AdminGetAudienceV1(JwtSecurityToken jwt, Guid audienceId)
        {
            var endpoint = "/audience/v1/";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + audienceId.ToString());

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"], endpoint) + audienceId.ToString());
        }

        public async Task<HttpResponseMessage> AdminGetClientV1(JwtSecurityToken jwt, Guid clientId)
        {
            var endpoint = "/client/v1/";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + clientId.ToString());

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"], endpoint) + clientId.ToString());
        }

        public async Task<HttpResponseMessage> AdminGetRoleV1(JwtSecurityToken jwt, Guid roleId)
        {
            var endpoint = "/role/v1/";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + roleId.ToString());

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"], endpoint) + roleId.ToString());

        }

        public async Task<HttpResponseMessage> AdminGetUserV1(JwtSecurityToken jwt, Guid userId)
        {
            var endpoint = "/user/v1/";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + userId.ToString());

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityApiUrls:AdminUrl"], _conf["IdentityApiUrls:AdminPath"], endpoint) + userId.ToString());
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> StsAccessTokenV1(string client, string audience, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", password),
                });

            var endpoint = "/oauth/v1/access";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> StsAccessTokenV2(string client, List<string> audiences, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("user", user),
                    new KeyValuePair<string, string>("password", password),
                });

            var endpoint = "/oauth/v2/access";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> StsAuthorizationCodeRequestV1(string client, string audience, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth/v1/authorization-code";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint) + content);
        }

        public async Task<HttpResponseMessage> StsAuthorizationCodeRequestV2(string client, string audience, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth/v2/authorization-code";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint) + content);
        }

        public async Task<HttpResponseMessage> StsAuthorizationCodeV1(string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            var endpoint = "/oauth/v1/authorization";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint) + content);
        }

        public async Task<HttpResponseMessage> StsAuthorizationCodeV2(string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            var endpoint = "/oauth/v2/authorization";

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint) + content);
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> StsClientCredentialsV1(string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth/v1/client";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> StsClientCredentialsV2(string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth/v2/client";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> StsRefreshTokenV1(string client, string audience, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth/v1/refresh";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> StsRefreshTokenV2(string client, List<string> audiences, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth/v2/refresh";

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _connect.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityApiUrls:StsUrl"], _conf["IdentityApiUrls:StsPath"], endpoint), content);
        }
    }
}
