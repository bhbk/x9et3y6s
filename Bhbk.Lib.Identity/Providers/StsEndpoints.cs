using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Providers
{
    public class StsClient : StsEndpoints
    {
        public StsClient(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
            : base(conf, situation, http) { }
    }

    //https://oauth.com/playground/
    public class StsEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionType _situation;
        protected readonly HttpClient _http;

        public StsEndpoints(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = situation;
            _conf = conf;

            if (situation == ExecutionType.Live)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }

            if (situation == ExecutionType.UnitTest)
                _http = http;
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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint + content);

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

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint + content);

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

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint + content);

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

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint + content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/device-code/
        public async Task<HttpResponseMessage> DeviceCode_GenerateV1(string issuer, string client, string code)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", issuer),
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("device_code", code),
                    new KeyValuePair<string, string>("grant_type", "device_code")
                });

            var endpoint = "/oauth2/v1/device";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_GenerateV2(string issuer, string client, string code)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", issuer),
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("device_code", code),
                    new KeyValuePair<string, string>("grant_type", "device_code")
                });

            var endpoint = "/oauth2/v2/device";

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

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

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV1(string jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV2(string jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV1(string jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV1(string jwt, string user, string token)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke/" + token;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV2(string jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV2(string jwt, string user, string token)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke/" + token;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
