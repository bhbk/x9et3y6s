using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Primitives.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public async Task<HttpResponseMessage> AdminAudienceCreateV1(JwtSecurityToken jwt, AudienceCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/audience/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminAudienceDeleteV1(JwtSecurityToken jwt, Guid audienceID)
        {
            var endpoint = "/audience/v1/" + audienceID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminAudienceGetV1(JwtSecurityToken jwt, string audience)
        {
            var endpoint = "/audience/v1/" + audience;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminClientCreateV1(JwtSecurityToken jwt, ClientCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminClientDeleteV1(JwtSecurityToken jwt, Guid clientID)
        {
            var endpoint = "/client/v1/" + clientID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminClientGetV1(JwtSecurityToken jwt, string client)
        {
            var endpoint = "/client/v1/" + client;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminLoginAddUserV1(JwtSecurityToken jwt, Guid loginID, Guid userID, UserLoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1/" + loginID.ToString() + "/add/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminLoginCreateV1(JwtSecurityToken jwt, LoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminLoginDeleteV1(JwtSecurityToken jwt, Guid loginID)
        {
            var endpoint = "/login/v1/" + loginID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminLoginGetV1(JwtSecurityToken jwt, string login)
        {
            var endpoint = "/login/v1/" + login;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

        }

        public async Task<HttpResponseMessage> AdminRoleAddToUserV1(JwtSecurityToken jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminRoleCreateV1(JwtSecurityToken jwt, RoleCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminRoleDeleteV1(JwtSecurityToken jwt, Guid roleID)
        {
            var endpoint = "/role/v1/" + roleID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminRoleGetV1(JwtSecurityToken jwt, string role)
        {
            var endpoint = "/role/v1/" + role;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

        }

        public async Task<HttpResponseMessage> AdminRoleRemoveFromUserV1(JwtSecurityToken jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminUserCreateV1(JwtSecurityToken jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminUserCreateV1NoConfirm(JwtSecurityToken jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/no-confirm";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> AdminUserDeleteV1(JwtSecurityToken jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString();

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminUserGetV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/user/v1/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> AdminUserSetPasswordV1(JwtSecurityToken jwt, UserAddPassword model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + model.Id.ToString() + "/set-password";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PutAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint + content);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);
        }

        public async Task<HttpResponseMessage> StsAuthorizationCodeV1(string client, string user, string redirectUri, string code)
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

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);
        }

        public async Task<HttpResponseMessage> StsAuthorizationCodeV2(string client, string user, string redirectUri, string code)
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

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint) + content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
        }

        public async Task<HttpResponseMessage> StsRefreshTokenGetListV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v1/refresh/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> StsRefreshTokenGetListV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v2/refresh/" + user;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.GetAsync(endpoint);

            return await _connect.GetAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> StsRefreshTokenDeleteV1(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth/v1/refresh/" + user + "/revoke/" + token;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> StsRefreshTokenDeleteV2(JwtSecurityToken jwt, string user, string token)
        {
            var endpoint = "/oauth/v2/refresh/" + user + "/revoke/" + token;

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> StsRefreshTokenDeleteAllV1(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v1/refresh/" + user + "/revoke";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
        }

        public async Task<HttpResponseMessage> StsRefreshTokenDeleteAllV2(JwtSecurityToken jwt, string user)
        {
            var endpoint = "/oauth/v2/refresh/" + user + "/revoke";

            _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.DeleteAsync(endpoint);

            return await _connect.DeleteAsync(
                string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
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

            _connect.DefaultRequestHeaders.Accept.Clear();
            _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_context == ContextType.UnitTest)
                return await _connect.PostAsync(endpoint, content);

            return await _connect.PostAsync(
                string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content);
        }
    }
}
