using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class AdminClient : AdminEndpoints
    {
        public AdminClient(IConfigurationRoot conf, ExecutionContext situation, HttpClient client)
            : base(conf, situation, client) { }
    }

    //https://oauth.com/playground/
    public class AdminEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionContext _situation;
        protected readonly HttpClient _client;

        public AdminEndpoints(IConfigurationRoot conf, ExecutionContext situation, HttpClient client)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = situation;
            _conf = conf;

            if (situation == ExecutionContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (situation == ExecutionContext.Testing)
                _client = client;
        }

        public async Task<HttpResponseMessage> Activity_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/activity/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/claim/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            var endpoint = "/claim/v1/" + claimID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetV1(string jwt, string claim)
        {
            var endpoint = "/claim/v1/" + claim;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/claim/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/claim/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_CreateV1(string jwt, ClientCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_DeleteV1(string jwt, Guid clientID)
        {
            var endpoint = "/client/v1/" + clientID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetV1(string jwt, string client)
        {
            var endpoint = "/client/v1/" + client;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Client_UpdateV1(string jwt, ClientModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/client/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            var endpoint = "/issuer/v1/" + issuerID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetClientsV1(string jwt, string issuer)
        {
            var endpoint = "/issuer/v1/" + issuer + "/clients";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetV1(string jwt, string issuer)
        {
            var endpoint = "/issuer/v1/" + issuer;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/issuer/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_CreateV1(string jwt, LoginCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_DeleteV1(string jwt, Guid loginID)
        {
            var endpoint = "/login/v1/" + loginID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetV1(string jwt, string login)
        {
            var endpoint = "/login/v1/" + login;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Login_UpdateV1(string jwt, LoginModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/login/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_AddToUserV1(string jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_CreateV1(string jwt, RoleCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            var endpoint = "/role/v1/" + roleID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetV1(string jwt, string role)
        {
            var endpoint = "/role/v1/" + role;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_RemoveFromUserV1(string jwt, Guid roleID, Guid userID)
        {
            var endpoint = "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Role_UpdateV1(string jwt, RoleModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/role/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddToLoginV1(string jwt, Guid userID, Guid loginID)
        {
            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-login/" + loginID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_AddPasswordV1(string jwt, Guid userID, UserAddPassword model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + userID.ToString() + "/add-password";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1(string jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserCreate model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/no-confirm";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetClientsV1(string jwt, string user)
        {
            var endpoint = "/user/v1/" + user + "/clients";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetLoginsV1(string jwt, string user)
        {
            var endpoint = "/user/v1/" + user + "/logins";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetV1(string jwt, string user)
        {
            var endpoint = "/user/v1/" + user;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetPageV1(string jwt, CascadePager model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/page";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_GetRolesV1(string jwt, string user)
        {
            var endpoint = "/user/v1/" + user + "/roles";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemoveFromLoginV1(string jwt, Guid userID, Guid loginID)
        {
            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-login/" + loginID.ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.DeleteAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            var endpoint = "/user/v1/" + userID.ToString() + "/remove-password";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, UserAddPassword model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1/" + userID.ToString() + "/set-password";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> User_UpdateV1(string jwt, UserModel model)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/user/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.PutAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionContext.Testing)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
