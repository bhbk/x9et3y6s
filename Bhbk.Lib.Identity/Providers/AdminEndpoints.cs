using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class AdminClient : AdminEndpoints
    {
        public AdminClient(IConfigurationRoot conf, ContextType situation)
            : base(conf, situation) { }


        public AdminClient(IConfigurationRoot conf, ContextType situation, HttpClient http)
    : base(conf, situation, http) { }
    }

    public class AdminTester : AdminEndpoints
    {
        public AdminTester(IConfigurationRoot conf, TestServer server)
            : base(conf, server) { }
    }

    //https://oauth.com/playground/
    public class AdminEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _situation;
        protected readonly HttpClient _http;

        public AdminEndpoints(IConfigurationRoot conf, ContextType situation)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _situation = situation;
            _conf = conf;
            _http = new HttpClient(connect);
        }

        public AdminEndpoints(IConfigurationRoot conf, ContextType situation, HttpClient http)
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

        public AdminEndpoints(IConfigurationRoot conf, TestServer server)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = ContextType.UnitTest;
            _conf = conf;
            _http = server.CreateClient();
        }

        public async Task<HttpResponseMessage> ActivityGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/activity/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientCreateV1(string jwt, ClientCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientDeleteV1(string jwt, Guid clientID)
        {
            var endpoint = "/client/v1/" + clientID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientGetV1(string jwt, string client)
        {
            var endpoint = "/client/v1/" + client;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> ClientUpdateV1(string jwt, ClientUpdate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PutAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerCreateV1(string jwt, IssuerCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerDeleteV1(string jwt, Guid issuerID)
        {
            var endpoint = "/issuer/v1/" + issuerID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerGetV1(string jwt, string issuer)
        {
            var endpoint = "/issuer/v1/" + issuer;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerGetClientsV1(string jwt, string issuer)
        {
            var endpoint = "/issuer/v1/" + issuer + "/clients";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> IssuerUpdateV1(string jwt, IssuerUpdate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PutAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginAddUserV1(string jwt, Guid loginID, Guid userID, UserLoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1/" + loginID.ToString() + "/add/" + userID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginCreateV1(string jwt, LoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginDeleteV1(string jwt, Guid loginID)
        {
            var endpoint = "/login/v1/" + loginID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginGetV1(string jwt, string login)
        {
            var endpoint = "/login/v1/" + login;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> LoginUpdateV1(string jwt, LoginUpdate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PutAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleAddToUserV1(string jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleCreateV1(string jwt, RoleCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleDeleteV1(string jwt, Guid roleID)
        {
            var endpoint = "/role/v1/" + roleID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleGetV1(string jwt, string role)
        {
            var endpoint = "/role/v1/" + role;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleRemoveFromUserV1(string jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> RoleUpdateV1(string jwt, RoleUpdate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PutAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserClaimCreateV1(string jwt, UserClaimCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + model.UserId + "/claim";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserCreateV1(string jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserCreateV1NoConfirm(string jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/no-confirm";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserDeleteV1(string jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.DeleteAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserGetV1(string jwt, string user)
        {
            var endpoint = "/user/v1/" + user;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserGetPagesV1(string jwt, TuplePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/pages";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserRemovePasswordV1(string jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString() + "/remove-password";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserSetPasswordV1(string jwt, UserAddPassword model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + model.Id.ToString() + "/set-password";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> UserUpdateV1(string jwt, UserUpdate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.PutAsync(endpoint, content);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            throw new NotSupportedException();
        }
    }
}
