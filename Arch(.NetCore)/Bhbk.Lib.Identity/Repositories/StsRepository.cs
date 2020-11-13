using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Repositories
{
    public class StsRepository
    {
        private readonly IConfiguration _conf;
        private readonly HttpClient _http;

        public StsRepository(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public StsRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;

            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{_conf["IdentityStsUrls:BaseApiUrl"]}/{_conf["IdentityStsUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /*
         * https://oauth.net/2/grant-types/authorization-code/
         */
        public async ValueTask<HttpResponseMessage> AuthCode_AskV1(AuthCodeAskV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            return await _http.GetAsync("oauth2/v1/acg-ask" + content);
        }

        public async ValueTask<HttpResponseMessage> AuthCode_AskV2(AuthCodeAskV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            return await _http.GetAsync("oauth2/v2/acg-ask" + content);
        }

        public async ValueTask<HttpResponseMessage> AuthCode_AuthV1(AuthCodeV1 model)
        {
            var content = Uri.EscapeUriString("?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&grant_type=" + model.grant_type
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state));

            return await _http.GetAsync("oauth2/v1/acg" + content);
        }

        public async ValueTask<HttpResponseMessage> AuthCode_AuthV2(AuthCodeV2 model)
        {
            var content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&grant_type=" + model.grant_type
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            return await _http.GetAsync("oauth2/v2/acg" + content);
        }

        /*
         * https://oauth.net/2/grant-types/client-credentials/
         * https://oauth.net/2/grant-types/refresh-token/
         */
        public async ValueTask<HttpResponseMessage> ClientCredential_AuthV1(ClientCredentialV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                });

            return await _http.PostAsync("oauth2/v1/ccg", content);
        }

        public async ValueTask<HttpResponseMessage> ClientCredential_AuthV2(ClientCredentialV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client",  model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                });

            return await _http.PostAsync("oauth2/v2/ccg", content);
        }

        public async ValueTask<HttpResponseMessage> ClientCredential_RefreshV1(RefreshTokenV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            return await _http.PostAsync("oauth2/v1/ccg-rt", content);
        }

        public async ValueTask<HttpResponseMessage> ClientCredential_RefreshV2(RefreshTokenV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            return await _http.PostAsync("oauth2/v2/ccg-rt", content);
        }

        /*
         * https://oauth.net/2/grant-types/device-code/
         */
        public async ValueTask<HttpResponseMessage> DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                });

            return await _http.PostAsync("oauth2/v1/dcg-ask", content);
        }

        public async ValueTask<HttpResponseMessage> DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user", model.user),
                });

            return await _http.PostAsync("oauth2/v2/dcg-ask", content);
        }

        public async ValueTask<HttpResponseMessage> DeviceCode_AuthV1(DeviceCodeV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                });

            return await _http.PostAsync("oauth2/v1/dcg", content);
        }

        public async ValueTask<HttpResponseMessage> DeviceCode_AuthV2(DeviceCodeV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                });

            return await _http.PostAsync("oauth2/v2/dcg", content);
        }

        /* 
         * https://oauth.net/2/grant-types/implicit/
         */
        public async ValueTask<HttpResponseMessage> Implicit_AuthV1(ImplicitV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&grant_type=" + model.grant_type
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            return await _http.GetAsync("oauth2/v1/ig" + content);
        }

        public async ValueTask<HttpResponseMessage> Implicit_AuthV2(ImplicitV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&grant_type=" + model.grant_type
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            return await _http.GetAsync("oauth2/v2/ig" + content);
        }

        /*
         * https://oauth.net/2/grant-types/password/
         * https://oauth.net/2/grant-types/refresh-token/
         */
        public async ValueTask<HttpResponseMessage> ResourceOwner_AuthV1Legacy(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            return await _http.PostAsync("oauth2/v1/ropg", content);
        }

        public async ValueTask<HttpResponseMessage> ResourceOwner_AuthV1(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            return await _http.PostAsync("oauth2/v1/ropg", content);
        }

        public async ValueTask<HttpResponseMessage> ResourceOwner_AuthV2(ResourceOwnerV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user", model.user),
                    new KeyValuePair<string, string>("password", model.password),
                });

            return await _http.PostAsync("oauth2/v2/ropg", content);
        }

        public async ValueTask<HttpResponseMessage> ResourceOwner_RefreshV1(RefreshTokenV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            return await _http.PostAsync("oauth2/v1/ropg-rt", content);
        }

        public async ValueTask<HttpResponseMessage> ResourceOwner_RefreshV2(RefreshTokenV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            return await _http.PostAsync("oauth2/v2/ropg-rt", content);
        }
    }
}
