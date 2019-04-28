using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Repositories
{
    public class StsRepository
    {
        private readonly IConfigurationRoot _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _client;

        public StsRepository(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal || instance == InstanceContext.IntegrationTest)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (instance == InstanceContext.UnitTest)
                _client = client;

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /*
         * https://oauth.net/2/grant-types/authorization-code/
         */
        public async Task<HttpResponseMessage> AuthCode_AskV1(AuthCodeAskV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            var endpoint = "/oauth2/v1/acg-ask";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_AskV2(AuthCodeAskV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            var endpoint = "/oauth2/v2/acg-ask";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_UseV1(AuthCodeV1 model)
        {
            var content = Uri.EscapeUriString("?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&grant_type=" + model.grant_type
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state));

            var endpoint = "/oauth2/v1/acg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_UseV2(AuthCodeV2 model)
        {
            var content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&grant_type=" + model.grant_type
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v2/acg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        /*
         * https://oauth.net/2/grant-types/client-credentials/
         * https://oauth.net/2/grant-types/refresh-token/
         */
        public async Task<HttpResponseMessage> ClientCredential_UseV1(ClientCredentialV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                });

            var endpoint = "/oauth2/v1/ccg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredential_UseV2(ClientCredentialV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client",  model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                });

            var endpoint = "/oauth2/v2/ccg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredentialRefresh_UseV1(RefreshTokenV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v1/ccg-rt";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredentialRefresh_UseV2(RefreshTokenV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v2/ccg-rt";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        /*
         * https://oauth.net/2/grant-types/device-code/
         */
        public async Task<HttpResponseMessage> DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                });

            var endpoint = "/oauth2/v1/dcg-ask";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user", model.user),
                });

            var endpoint = "/oauth2/v2/dcg-ask";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_UseV1(DeviceCodeV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                });

            var endpoint = "/oauth2/v1/dcg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_UseV2(DeviceCodeV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                });

            var endpoint = "/oauth2/v2/dcg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        /* 
         * https://oauth.net/2/grant-types/implicit/
         */
        public async Task<HttpResponseMessage> Implicit_UseV1(ImplicitV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&grant_type=" + model.grant_type
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v1/ig";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Implicit_UseV2(ImplicitV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&grant_type=" + model.grant_type
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=" + model.response_type
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v2/ig";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        /*
         * https://oauth.net/2/grant-types/password/
         * https://oauth.net/2/grant-types/refresh-token/
         */
        public async Task<HttpResponseMessage> ResourceOwner_UseV1Legacy(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v1/ropg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwner_UseV1(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v1/ropg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwner_UseV2(ResourceOwnerV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("user", model.user),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v2/ropg";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwnerRefresh_UseV1(RefreshTokenV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v1/ropg-rt";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwnerRefresh_UseV2(RefreshTokenV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", model.grant_type),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v2/ropg-rt";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
