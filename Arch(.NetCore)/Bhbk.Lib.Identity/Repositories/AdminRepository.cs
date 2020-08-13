using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public AdminRepository(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AdminRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }
            else if (instance == InstanceContext.End2EndTest)
                _http = http;
            else
                throw new NotImplementedException();

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/activity/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, string activityValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/activity/v1/" + activityValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_CreateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/audience/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshesV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshV1(string jwt, Guid audienceID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/audience/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRefreshesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRolesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/audience/v1/" + audienceValue + "/roles";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_SetPasswordV1(string jwt, Guid audienceID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/audience/v1/" + audienceID.ToString() + "/set-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Audience_UpdateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/audience/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, string claimValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/claim/v1/" + claimValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/claim/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetAudiencesV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue + "/audiences";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/issuer/v1/" + issuerValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetKeysV1(string jwt, List<string> model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1/keys";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/issuer/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Login_CreateV1(string jwt, LoginV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Login_DeleteV1(string jwt, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Login_GetV1(string jwt, string loginValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/login/v1/" + loginValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Login_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Login_UpdateV1(string jwt, LoginV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/login/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> MOTD_GetV1(string jwt, string motdValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/motd/v1/" + motdValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> MOTD_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/motd/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Role_CreateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, string roleValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/role/v1/" + roleValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Role_UpdateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/role/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_AddToClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-claim/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_AddToLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_AddToRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/add-to-role/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/no-confirm";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshesV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshV1(string jwt, Guid userID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/refresh/" + refreshID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetAudiencesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/audiences";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetClaimsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/claims";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetLoginsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/logins";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetRefreshesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/refreshes";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetRolesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue + "/roles";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-claim/" + claimID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-login/" + loginID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-from-role/" + roleID.ToString();

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/remove-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1/" + userID.ToString() + "/set-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/user/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> User_VerifyV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/user/v1/" + userID.ToString() + "/verify";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
