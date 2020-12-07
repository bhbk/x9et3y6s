using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class AdminService : IAdminService
    {
        public IOAuth2JwtGrant Grant { get; set; }
        public AdminServiceRepository Endpoints { get; }

        public AdminService()
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(), 
                  InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public AdminService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public AdminService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Endpoints = new AdminServiceRepository(conf, instance, http);
        }

        public async ValueTask<ActivityV1> Activity_GetV1(string activityValue)
        {
            var response = await Endpoints.Activity_GetV1(Grant.AccessToken.RawData, activityValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ActivityV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<ActivityV1>> Activity_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Activity_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<ActivityV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_AddToRoleV1(Guid audienceID, Guid roleID)
        {
            var response = await Endpoints.Audience_AddToRoleV1(Grant.AccessToken.RawData, audienceID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<AudienceV1> Audience_CreateV1(AudienceV1 model)
        {
            var response = await Endpoints.Audience_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AudienceV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteV1(Guid audienceID)
        {
            var response = await Endpoints.Audience_DeleteV1(Grant.AccessToken.RawData, audienceID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteRefreshesV1(Guid audienceID)
        {
            var response = await Endpoints.Audience_DeleteRefreshesV1(Grant.AccessToken.RawData, audienceID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteRefreshV1(Guid audienceID, Guid refreshID)
        {
            var response = await Endpoints.Audience_DeleteRefreshV1(Grant.AccessToken.RawData, audienceID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<AudienceV1>> Audience_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Audience_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<AudienceV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<AudienceV1> Audience_GetV1(string audienceValue)
        {
            var response = await Endpoints.Audience_GetV1(Grant.AccessToken.RawData, audienceValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AudienceV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> Audience_GetRefreshesV1(string audienceValue)
        {
            var response = await Endpoints.Audience_GetRefreshesV1(Grant.AccessToken.RawData, audienceValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RefreshV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RoleV1>> Audience_GetRolesV1(string audienceValue)
        {
            var response = await Endpoints.Audience_GetRolesV1(Grant.AccessToken.RawData, audienceValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RoleV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_RemoveFromRoleV1(Guid audienceID, Guid roleID)
        {
            var response = await Endpoints.Audience_RemoveFromRoleV1(Grant.AccessToken.RawData, audienceID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_RemovePasswordV1(Guid userID)
        {
            var response = await Endpoints.Audience_RemovePasswordV1(Grant.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Audience_SetPasswordV1(Guid audienceID, PasswordAddV1 model)
        {
            var response = await Endpoints.Audience_SetPasswordV1(Grant.AccessToken.RawData, audienceID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<AudienceV1> Audience_UpdateV1(AudienceV1 model)
        {
            var response = await Endpoints.Audience_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AudienceV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClaimV1> Claim_CreateV1(ClaimV1 model)
        {
            var response = await Endpoints.Claim_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClaimV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Claim_DeleteV1(Guid claimID)
        {
            var response = await Endpoints.Claim_DeleteV1(Grant.AccessToken.RawData, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClaimV1> Claim_GetV1(string claimValue)
        {
            var response = await Endpoints.Claim_GetV1(Grant.AccessToken.RawData, claimValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClaimV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<ClaimV1>> Claim_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Claim_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<ClaimV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClaimV1> Claim_UpdateV1(ClaimV1 model)
        {
            var response = await Endpoints.Claim_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClaimV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IssuerV1> Issuer_CreateV1(IssuerV1 model)
        {
            var response = await Endpoints.Issuer_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IssuerV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Issuer_DeleteV1(Guid issuerID)
        {
            var response = await Endpoints.Issuer_DeleteV1(Grant.AccessToken.RawData, issuerID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<AudienceV1>> Issuer_GetAudiencesV1(string issuerValue)
        {
            var response = await Endpoints.Issuer_GetAudiencesV1(Grant.AccessToken.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<AudienceV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IssuerV1> Issuer_GetV1(string issuerValue)
        {
            var response = await Endpoints.Issuer_GetV1(Grant.AccessToken.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IssuerV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<IssuerV1>> Issuer_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Issuer_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<IssuerV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<Dictionary<Guid, string>> Issuer_GetKeysV1(List<string> model)
        {
            var response = await Endpoints.Issuer_GetKeysV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<Dictionary<Guid, string>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IssuerV1> Issuer_UpdateV1(IssuerV1 model)
        {
            var response = await Endpoints.Issuer_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IssuerV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<LoginV1> Login_CreateV1(LoginV1 model)
        {
            var response = await Endpoints.Login_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<LoginV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Login_DeleteV1(Guid loginID)
        {
            var response = await Endpoints.Login_DeleteV1(Grant.AccessToken.RawData, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<LoginV1> Login_GetV1(string loginValue)
        {
            var response = await Endpoints.Login_GetV1(Grant.AccessToken.RawData, loginValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<LoginV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<LoginV1>> Login_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Login_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<LoginV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<LoginV1> Login_UpdateV1(LoginV1 model)
        {
            var response = await Endpoints.Login_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<LoginV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<MOTDTssV1> MOTD_GetV1(string motdValue)
        {
            var response = await Endpoints.MOTD_GetV1(Grant.AccessToken.RawData, motdValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<MOTDTssV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<MOTDTssV1>> MOTD_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.MOTD_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<MOTDTssV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<RoleV1> Role_CreateV1(RoleV1 model)
        {
            var response = await Endpoints.Role_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<RoleV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Role_DeleteV1(Guid roleID)
        {
            var response = await Endpoints.Role_DeleteV1(Grant.AccessToken.RawData, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<RoleV1> Role_GetV1(string roleValue)
        {
            var response = await Endpoints.Role_GetV1(Grant.AccessToken.RawData, roleValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<RoleV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<RoleV1>> Role_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.Role_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<RoleV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<RoleV1> Role_UpdateV1(RoleV1 model)
        {
            var response = await Endpoints.Role_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<RoleV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_AddToClaimV1(Guid userID, Guid claimID)
        {
            var response = await Endpoints.User_AddToClaimV1(Grant.AccessToken.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_AddToLoginV1(Guid userID, Guid loginID)
        {
            var response = await Endpoints.User_AddToLoginV1(Grant.AccessToken.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_AddToRoleV1(Guid userID, Guid roleID)
        {
            var response = await Endpoints.User_AddToRoleV1(Grant.AccessToken.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> User_CreateV1(UserV1 model)
        {
            var response = await Endpoints.User_CreateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> User_CreateV1NoConfirm(UserV1 model)
        {
            var response = await Endpoints.User_CreateV1NoConfirm(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_DeleteV1(Guid userID)
        {
            var response = await Endpoints.User_DeleteV1(Grant.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshesV1(Guid userID)
        {
            var response = await Endpoints.User_DeleteRefreshesV1(Grant.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = await Endpoints.User_DeleteRefreshV1(Grant.AccessToken.RawData, userID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<ClaimV1>> User_GetClaimsV1(string userValue)
        {
            var response = await Endpoints.User_GetClaimsV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<ClaimV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<AudienceV1>> User_GetAudiencesV1(string userValue)
        {
            var response = await Endpoints.User_GetAudiencesV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<AudienceV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<LoginV1>> User_GetLoginsV1(string userValue)
        {
            var response = await Endpoints.User_GetLoginsV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<LoginV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshV1>> User_GetRefreshesV1(string userValue)
        {
            var response = await Endpoints.User_GetRefreshesV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RefreshV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<IEnumerable<RoleV1>> User_GetRolesV1(string userValue)
        {
            var response = await Endpoints.User_GetRolesV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<IEnumerable<RoleV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> User_GetV1(string userValue)
        {
            var response = await Endpoints.User_GetV1(Grant.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DataStateV1Result<UserV1>> User_GetV1(DataStateV1 model)
        {
            var response = await Endpoints.User_GetV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DataStateV1Result<UserV1>>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromClaimV1(Guid userID, Guid claimID)
        {
            var response = await Endpoints.User_RemoveFromClaimV1(Grant.AccessToken.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromLoginV1(Guid userID, Guid loginID)
        {
            var response = await Endpoints.User_RemoveFromLoginV1(Grant.AccessToken.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromRoleV1(Guid userID, Guid roleID)
        {
            var response = await Endpoints.User_RemoveFromRoleV1(Grant.AccessToken.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_RemovePasswordV1(Guid userID)
        {
            var response = await Endpoints.User_RemovePasswordV1(Grant.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> User_SetPasswordV1(Guid userID, PasswordAddV1 model)
        {
            var response = await Endpoints.User_SetPasswordV1(Grant.AccessToken.RawData, userID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserV1> User_UpdateV1(UserV1 model)
        {
            var response = await Endpoints.User_UpdateV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }
    }
}
