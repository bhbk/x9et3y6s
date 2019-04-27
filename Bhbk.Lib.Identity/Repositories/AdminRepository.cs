using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    //https://oauth.com/playground/
    public class AdminRepository
    {
        private readonly IConfigurationRoot _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _client;

        public AdminRepository(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal)
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

        public async Task<HttpResponseMessage> Activity_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/activity/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetV1(string jwt, string claimValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_CreateV1(string jwt, ClientCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteV1(string jwt, Guid clientID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteRefreshesV1(string jwt, Guid clientID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteRefreshV1(string jwt, Guid clientID, Guid refreshID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetV1(string jwt, string clientValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetRefreshesV1(string jwt, string clientValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_UpdateV1(string jwt, ClientModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetClientsV1(string jwt, string issuerValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue + "/clients";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetV1(string jwt, string issuerValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_CreateV1(string jwt, LoginCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_DeleteV1(string jwt, Guid loginID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetV1(string jwt, string loginValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_UpdateV1(string jwt, LoginModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_AddUserV1(string jwt, Guid roleID, Guid userID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_CreateV1(string jwt, RoleCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetV1(string jwt, string roleValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_RemoveUserV1(string jwt, Guid roleID, Guid userID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_UpdateV1(string jwt, RoleModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddPasswordV1(string jwt, Guid userID, UserAddPassword model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/" + userID.ToString() + "/add-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1(string jwt, UserCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/no-confirm";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteRefreshesV1(string jwt, Guid userID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteRefreshV1(string jwt, Guid userID, Guid refreshID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetClaimsV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/claims";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetClientsV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/clients";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetLoginsV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/logins";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetRefreshesV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetRolesV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/roles";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetV1(string jwt, string userValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetV1(string jwt, CascadePager model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemoveLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, UserAddPassword model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/" + userID.ToString() + "/set-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_UpdateV1(string jwt, UserModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
