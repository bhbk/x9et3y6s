using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Sts;
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
        public StsClient(IConfigurationRoot conf, ExecutionType situation, HttpClient client)
            : base(conf, situation, client) { }
    }

    //https://oauth.com/playground/
    public class StsEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionType _situation;
        protected readonly HttpClient _client;

        public StsEndpoints(IConfigurationRoot conf, ExecutionType situation, HttpClient client)
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

                _client = new HttpClient(connect);
            }

            if (situation == ExecutionType.UnitTest)
                _client = client;
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> AuthCode_AskV1(AuthCodeAskV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=code"
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            var endpoint = "/oauth2/v1/authorize-ask";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_AskV2(AuthCodeAskV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=code"
                + "&scope=" + HttpUtility.UrlEncode(model.scope);

            var endpoint = "/oauth2/v2/authorize-ask";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_UseV1(AuthCodeV1 model)
        {
            var content = Uri.EscapeUriString("?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&grant_type=authorization_code"
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state));

            var endpoint = "/oauth2/v1/authorize";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> AuthCode_UseV2(AuthCodeV2 model)
        {
            var content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&grant_type=authorization_code"
                + "&code=" + HttpUtility.UrlEncode(model.code)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v2/authorize";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> ClientCredential_UseV1(ClientCredentialV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth2/v1/client";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCredential_UseV2(ClientCredentialV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("client_secret", model.client_secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            var endpoint = "/oauth2/v2/client";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/device-code/
        public async Task<HttpResponseMessage> DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("grant_type", "device_code")
                });

            var endpoint = "/oauth2/v1/device-ask";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("user", model.user),
                    new KeyValuePair<string, string>("grant_type", "device_code")
                });

            var endpoint = "/oauth2/v2/device-ask";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_DecideV1(string jwt, string code, string action)
        {
            var endpoint = "/oauth2/v1/device/" + code + "/" + action;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_DecideV2(string jwt, string code, string action)
        {
            var endpoint = "/oauth2/v2/device/" + code + "/" + action;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_UseV1(DeviceCodeV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                    new KeyValuePair<string, string>("grant_type", "device_code"),
                });

            var endpoint = "/oauth2/v1/device";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> DeviceCode_UseV2(DeviceCodeV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("user_code", model.user_code),
                    new KeyValuePair<string, string>("device_code", model.device_code),
                    new KeyValuePair<string, string>("grant_type", "device_code"),
                });

            var endpoint = "/oauth2/v2/device";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/implicit/
        public async Task<HttpResponseMessage> Implicit_UseV1(ImplicitV1 model)
        {
            string content = "?issuer_id=" + HttpUtility.UrlEncode(model.issuer_id)
                + "&client_id=" + HttpUtility.UrlEncode(model.client_id)
                + "&username=" + HttpUtility.UrlEncode(model.username)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=token"
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v1/implicit";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Implicit_UseV2(ImplicitV2 model)
        {
            string content = "?issuer=" + HttpUtility.UrlEncode(model.issuer)
                + "&client=" + HttpUtility.UrlEncode(model.client)
                + "&user=" + HttpUtility.UrlEncode(model.user)
                + "&redirect_uri=" + HttpUtility.UrlEncode(model.redirect_uri)
                + "&response_type=token"
                + "&scope=" + HttpUtility.UrlEncode(model.scope)
                + "&state=" + HttpUtility.UrlEncode(model.state);

            var endpoint = "/oauth2/v2/implicit";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint + content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> RefreshToken_DeleteV1(string jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV1(string jwt, string user, string token)
        {
            var endpoint = "/oauth2/v1/refresh/" + user + "/revoke/" + token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV2(string jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_DeleteV2(string jwt, string user, string token)
        {
            var endpoint = "/oauth2/v2/refresh/" + user + "/revoke/" + token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV1(string jwt, string user)
        {
            var endpoint = "/oauth2/v1/refresh/" + user;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_GetListV2(string jwt, string user)
        {
            var endpoint = "/oauth2/v2/refresh/" + user;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_UseV1(RefreshTokenV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v1/refresh";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RefreshToken_UseV2(RefreshTokenV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", model.refresh_token),
                });

            var endpoint = "/oauth2/v2/refresh";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> ResourceOwner_UseV1Legacy(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v1/access";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwner_UseV1(ResourceOwnerV1 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer_id", model.issuer_id),
                    new KeyValuePair<string, string>("client_id", model.client_id),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", model.username),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v1/access";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ResourceOwner_UseV2(ResourceOwnerV2 model)
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("issuer", model.issuer),
                    new KeyValuePair<string, string>("client", model.client),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("user", model.user),
                    new KeyValuePair<string, string>("password", model.password),
                });

            var endpoint = "/oauth2/v2/access";

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
