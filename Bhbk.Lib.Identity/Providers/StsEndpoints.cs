﻿using Bhbk.Lib.Core.Primitives.Enums;
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

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class StsClient : StsEndpoints
    {
        public StsClient(IConfigurationRoot conf, ContextType situation, HttpClient http)
            : base(conf, situation, http) { }
    }

    public class StsTester : StsEndpoints
    {
        public StsTester(IConfigurationRoot conf, TestServer server)
            : base(conf, server) { }
    }

    //https://oauth.com/playground/
    public class StsEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _situation;
        protected readonly HttpClient _http;

        public StsEndpoints(IConfigurationRoot conf, ContextType situation, HttpClient http)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _situation = situation;
            _conf = conf;
            _http = http;
        }

        public StsEndpoints(IConfigurationRoot conf, TestServer server)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = ContextType.UnitTest;
            _conf = conf;
            _http = server.CreateClient();
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> AccessToken_GenerateV1(string issuer, string client, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", issuer),
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", password),
                });

            var endpoint = "/oauth2/v1/access";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AccessToken_GenerateV1_CompatibilityModeIssuer(string client, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", password),
                });

            var endpoint = "/oauth2/v1/access";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AccessToken_GenerateV2(string issuer, List<string> clients, string user, string password)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", issuer),
                    new KeyValuePair<string, string>("client", string.Join(",", clients.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("user", user),
                    new KeyValuePair<string, string>("password", password),
                });

            var endpoint = "/oauth2/v2/access";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> AuthorizationCode_RequestV1(string issuer, string client, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?issuer_id=" + issuer
                + "&client_id=" + client
                + "&username=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth2/v1/authorization-code";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint + content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCode_RequestV2(string issuer, string client, string user, string redirectUri, string scope)
        {
            string content = HttpUtility.UrlPathEncode("?issuer=" + issuer
                + "&client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            var endpoint = "/oauth2/v2/authorization-code";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint + content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCode_GenerateV1(string issuer, string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?issuer_id=" + issuer
                + "&client_id=" + client
                + "&username=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=code"
                + "&code=" + code);

            var endpoint = "/oauth2/v1/authorization";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint + content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthorizationCode_GenerateV2(string issuer, string client, string user, string redirectUri, string code)
        {
            var content = HttpUtility.UrlPathEncode("?issuer=" + issuer
                + "&client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=code"
                + "&code=" + code);

            var endpoint = "/oauth2/v2/authorization";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint + content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> ClientCredentials_GenerateV1(string issuer, string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", issuer),
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth2/v1/client";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredentials_GenerateV2(string issuer, string client, string secret)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", issuer),
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth2/v2/client";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV1(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke/" + token;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV2(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke/" + token;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteAllV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteAllV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> RefreshToken_GenerateV1(string issuer, string client, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", issuer),
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth2/v1/refresh";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GenerateV2(string issuer, List<string> clients, string refresh)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", issuer),
                    new KeyValuePair<string, string>("client", string.Join(",", clients.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            var endpoint = "/oauth2/v2/refresh";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }
    }
}
