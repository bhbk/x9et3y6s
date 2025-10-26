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

namespace Bhbk.Lib.Identity.Services
{
    public class AdminServiceRepository
    {
        private readonly HttpClient _http;

        public AdminServiceRepository(IConfiguration conf, InstanceContext env, HttpClient http)
        {
            if (env == InstanceContext.DeployedOrLocal
                || env == InstanceContext.End2EndTest)
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

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("activities/v1/users/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Activity_GetV1(string jwt, string activityValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("activities/v1/users/" + activityValue);
        }

        public async ValueTask<HttpResponseMessage> Audience_AddToRoleV1(string jwt, Guid audienceID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audiences/v1/" + audienceID.ToString() + "/add-to-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_CreateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("audiences/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audiences/v1/" + audienceID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshesV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audiences/v1/" + audienceID.ToString() + "/refresh");
        }

        public async ValueTask<HttpResponseMessage> Audience_DeleteRefreshV1(string jwt, Guid audienceID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audiences/v1/" + audienceID.ToString() + "/refresh/" + refreshID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audiences/v1/" + audienceValue);
        }

        public async ValueTask<HttpResponseMessage> Audience_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("audiences/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRefreshesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audiences/v1/" + audienceValue + "/refreshes");
        }

        public async ValueTask<HttpResponseMessage> Audience_GetRolesV1(string jwt, string audienceValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audiences/v1/" + audienceValue + "/roles");
        }

        public async ValueTask<HttpResponseMessage> Audience_RemoveFromRoleV1(string jwt, Guid audienceID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("audiences/v1/" + audienceID.ToString() + "/remove-from-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Audience_RemovePasswordV1(string jwt, Guid audienceID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("audiences/v1/" + audienceID.ToString() + "/remove-password");
        }

        public async ValueTask<HttpResponseMessage> Audience_SetPasswordV1(string jwt, Guid audienceID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("audiences/v1/" + audienceID.ToString() + "/set-password",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Audience_UpdateV1(string jwt, AudienceV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("audiences/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_CreateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("claims/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_DeleteV1(string jwt, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("claims/v1/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, string claimValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("claims/v1/" + claimValue);
        }

        public async ValueTask<HttpResponseMessage> Claim_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("claims/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Claim_UpdateV1(string jwt, ClaimV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("claims/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_CreateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuers/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_DeleteV1(string jwt, Guid issuerID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("issuers/v1/" + issuerID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetAudiencesV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("issuers/v1/" + issuerValue + "/audiences");
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, string issuerValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("issuers/v1/" + issuerValue);
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuers/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_GetKeysV1(string jwt, List<string> model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("issuers/v1/keys",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Issuer_UpdateV1(string jwt, IssuerV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("issuers/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> LoginProvider_CreateV1(string jwt, LoginProviderV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("login-providers/v1", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> LoginProvider_DeleteV1(string jwt, Guid loginProviderID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("login-providers/v1/" + loginProviderID.ToString());
        }

        public async ValueTask<HttpResponseMessage> LoginProvider_GetV1(string jwt, string loginProviderValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("login-providers/v1/" + loginProviderValue);
        }

        public async ValueTask<HttpResponseMessage> LoginProvider_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("login-providers/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> LoginProvider_UpdateV1(string jwt, LoginProviderV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("login-providers/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Quote_GetV1(string jwt, string motdValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("quotes/v1/" + motdValue);
        }

        public async ValueTask<HttpResponseMessage> Quote_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("quotes/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_CreateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("roles/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_DeleteV1(string jwt, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("roles/v1/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, string roleValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("roles/v1/" + roleValue);
        }

        public async ValueTask<HttpResponseMessage> Role_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("roles/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Role_UpdateV1(string jwt, RoleV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("roles/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_AddToClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userID.ToString() + "/add-to-claim/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_AddToLoginProviderV1(string jwt, Guid userID, Guid loginProviderID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userID.ToString() + "/add-to-login-provider/" + loginProviderID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_AddToRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userID.ToString() + "/add-to-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("users/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_CreateV1NoConfirm(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("users/v1/no-confirm",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_DeleteV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshesV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString() + "/refresh");
        }

        public async ValueTask<HttpResponseMessage> User_DeleteRefreshV1(string jwt, Guid userID, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString() + "/refresh/" + refreshID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_GetAudiencesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue + "/audiences");
        }

        public async ValueTask<HttpResponseMessage> User_GetClaimsV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue + "/claims");
        }

        public async ValueTask<HttpResponseMessage> User_GetLoginProvidersV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue + "/login-providers");
        }

        public async ValueTask<HttpResponseMessage> User_GetRefreshesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue + "/refreshes");
        }

        public async ValueTask<HttpResponseMessage> User_GetRolesV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue + "/roles");
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, string userValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userValue);
        }

        public async ValueTask<HttpResponseMessage> User_GetV1(string jwt, PagerState model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("users/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromClaimV1(string jwt, Guid userID, Guid claimID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString() + "/remove-from-claim/" + claimID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromLoginProviderV1(string jwt, Guid userID, Guid loginProviderID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString() + "/remove-from-login-provider/" + loginProviderID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemoveFromRoleV1(string jwt, Guid userID, Guid roleID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("users/v1/" + userID.ToString() + "/remove-from-role/" + roleID.ToString());
        }

        public async ValueTask<HttpResponseMessage> User_RemovePasswordV1(string jwt, Guid userID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("users/v1/" + userID.ToString() + "/remove-password");
        }

        public async ValueTask<HttpResponseMessage> User_SetPasswordV1(string jwt, Guid userID, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("users/v1/" + userID.ToString() + "/set-password",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> User_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("users/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }
    }
}
