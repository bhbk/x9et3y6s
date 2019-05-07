using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    public class AdminRepository
    {
        private readonly IConfiguration _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _http;

        public AdminRepository()
            : this(InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AdminRepository(InstanceContext instance, HttpClient http)
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal || instance == InstanceContext.IntegrationTest)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }

            if (instance == InstanceContext.UnitTest)
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> Activity_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/activity/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetV1(string jwt, string claimValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_CreateV1(string jwt, ClientCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteV1(string jwt, Guid clientID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteRefreshesV1(string jwt, Guid clientID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteRefreshV1(string jwt, Guid clientID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetV1(string jwt, string clientValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetRefreshesV1(string jwt, string clientValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/client/v1/" + clientValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_UpdateV1(string jwt, ClientModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/client/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetClientsV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue + "/clients";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_CreateV1(string jwt, LoginCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_DeleteV1(string jwt, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetV1(string jwt, string loginValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_UpdateV1(string jwt, LoginModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_CreateV1(string jwt, RoleCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetV1(string jwt, string roleValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_UpdateV1(string jwt, RoleModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddToClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-claim/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddToLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddToRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-role/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1(string jwt, UserCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserCreate model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/no-confirm";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteRefreshesV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteRefreshV1(string jwt, Guid userID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetMOTDsV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/msg-of-the-day/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetClaimsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/claims";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetClientsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/clients";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetLoginsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/logins";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetRefreshesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetRolesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/roles";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetV1(string jwt, CascadePager model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemoveFromClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-claim/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemoveFromLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemoveFromRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-role/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-password";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, UserAddPassword model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/" + userID.ToString() + "/set-password";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_UpdateV1(string jwt, UserModel model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
