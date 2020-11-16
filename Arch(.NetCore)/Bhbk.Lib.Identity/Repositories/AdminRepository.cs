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
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    public class AdminRepository
    {
        private readonly HttpClient _http;

        public AdminRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{conf["IdentityAdminUrls:BaseApiUrl"]}/{conf["IdentityAdminUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("activity/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, string activityValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("activity/v1/" + activityValue);
        }

        public async ValueTask<HttpResponseMessage> Audience_AddToRoleV1(string jwt, Guid audienceID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audience/v1/" + audienceID.ToString() + "/add-to-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_CreateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("audience/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audience/v1/" + audienceID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshesV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audience/v1/" + audienceID.ToString() + "/refresh");
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshV1(string jwt, Guid audienceID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audience/v1/" + audienceID.ToString() + "/refresh/" + refreshID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audience/v1/" + audienceValue);
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("audience/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRefreshesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audience/v1/" + audienceValue + "/refreshes");
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRolesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audience/v1/" + audienceValue + "/roles");
        }

        public async ValueTask<HttpResponseMessage> Audience_RemoveFromRoleV1(string jwt, Guid audienceID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audience/v1/" + audienceID.ToString() + "/remove-from-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_RemovePasswordV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audience/v1/" + audienceID.ToString() + "/remove-password");
        }

        public async ValueTask<HttpResponseMessage> Audience_SetPasswordV1(string jwt, Guid audienceID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("audience/v1/" + audienceID.ToString() + "/set-password",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_UpdateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("audience/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("claim/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("claim/v1/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, string claimValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("claim/v1/" + claimValue);
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("claim/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("claim/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuer/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("issuer/v1/" + issuerID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetAudiencesV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("issuer/v1/" + issuerValue + "/audiences");
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("issuer/v1/" + issuerValue);
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuer/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetKeysV1(string jwt, List<string> model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuer/v1/keys",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("issuer/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Login_CreateV1(string jwt, LoginV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("login/v1", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Login_DeleteV1(string jwt, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("login/v1/" + loginID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Login_GetV1(string jwt, string loginValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("login/v1/" + loginValue);
        }

        public async ValueTask<HttpResponseMessage> Login_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("login/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Login_UpdateV1(string jwt, LoginV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("login/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> MOTD_GetV1(string jwt, string motdValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("motd/v1/" + motdValue);
        }

        public async ValueTask<HttpResponseMessage> MOTD_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("motd/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_CreateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("role/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("role/v1/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, string roleValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("role/v1/" + roleValue);
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("role/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_UpdateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("role/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_AddToClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userID.ToString() + "/add-to-claim/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_AddToLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userID.ToString() + "/add-to-login/" + loginID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_AddToRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userID.ToString() + "/add-to-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("user/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("user/v1/no-confirm",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshesV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString() + "/refresh");
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshV1(string jwt, Guid userID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString() + "/refresh/" + refreshID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_GetAudiencesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue + "/audiences");
        }

        public async ValueTask<HttpResponseMessage> User_GetClaimsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue + "/claims");
        }

        public async ValueTask<HttpResponseMessage> User_GetLoginsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue + "/logins");
        }

        public async ValueTask<HttpResponseMessage> User_GetRefreshesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue + "/refreshes");
        }

        public async ValueTask<HttpResponseMessage> User_GetRolesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue + "/roles");
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userValue);
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("user/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString() + "/remove-from-claim/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromLoginV1(string jwt, Guid userID, Guid loginID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString() + "/remove-from-login/" + loginID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("user/v1/" + userID.ToString() + "/remove-from-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userID.ToString() + "/remove-password");
        }

        public async ValueTask<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("user/v1/" + userID.ToString() + "/set-password",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("user/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_VerifyV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("user/v1/" + userID.ToString() + "/verify");
        }
    }
}
