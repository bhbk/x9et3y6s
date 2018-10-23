using Bhbk.Lib.Core.Primitives.Enums;
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

namespace Bhbk.Lib.Identity.Interop
{
    public class StsClient : StsHelper
    {
        public StsClient(IConfigurationRoot conf, ContextType context)
            : base(conf, context) { }
    }

    public class StsTester : StsHelper
    {
        public StsTester(IConfigurationRoot conf, TestServer connect)
            : base(conf, connect) { }
    }

    //https://oauth.com/playground/
    public class StsHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public StsHelper(IConfigurationRoot conf, ContextType context)
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

        public StsHelper(IConfigurationRoot conf, TestServer connect)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _context = ContextType.UnitTest;
            _conf = conf;
            _connect = connect.CreateClient();
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> AccessTokenV1(string client, string audience, string user, string password)
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AccessTokenV2(string client, List<string> audiences, string user, string password)
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> AuthorizationCodeRequestV1(string client, string audience, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth/v1/authorization-code";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCodeRequestV2(string client, string audience, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth/v2/authorization-code";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCodeV1(string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            var endpoint = "/oauth/v1/authorization";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCodeV2(string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            var endpoint = "/oauth/v2/authorization";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> ClientCredentialsV1(string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth/v1/client";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredentialsV2(string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth/v2/client";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenGetListV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v1/refresh/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenGetListV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v2/refresh/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenDeleteV1(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth/v1/refresh/" + user + "/revoke/" + token;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenDeleteV2(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth/v2/refresh/" + user + "/revoke/" + token;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenDeleteAllV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v1/refresh/" + user + "/revoke";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenDeleteAllV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v2/refresh/" + user + "/revoke";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> RefreshTokenV1(string client, string audience, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth/v1/refresh";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshTokenV2(string client, List<string> audiences, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth/v2/refresh";

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            if (_context == ContextType.IntegrationTest || _context == ContextType.Live)
                return await _connect.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }
    }
}
