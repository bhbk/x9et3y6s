using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class AdminService : IAdminService
    {
        private readonly IOAuth2JwtGrant _ropg;
        private readonly AdminRepository _http;

        public AdminService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AdminService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _ropg = new ResourceOwnerGrantV2(conf, instance, http);
            _http = new AdminRepository(conf, instance, http);
        }

        public JwtSecurityToken AccessToken
        {
            get { return _ropg.AccessToken; }
            set { _ropg.AccessToken = value; }
        }

        public AdminRepository Http
        {
            get { return _http; }
        }

        public async ValueTask<ActivityModel> Activity_GetV1(string activityValue)
        {
            var response = await Http.Activity_GetV1(_ropg.AccessToken.RawData, activityValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ActivityModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<ActivityModel>> Activity_GetV1(PageStateTypeC model)
        {
            var response = await Http.Activity_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<ActivityModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<AudienceModel> Audience_CreateV1(AudienceCreate model)
        {
            var response = await Http.Audience_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<AudienceModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteV1(Guid audienceID)
        {
            var response = await Http.Audience_DeleteV1(_ropg.AccessToken.RawData, audienceID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteRefreshesV1(Guid audienceID)
        {
            var response = await Http.Audience_DeleteRefreshesV1(_ropg.AccessToken.RawData, audienceID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Audience_DeleteRefreshV1(Guid audienceID, Guid refreshID)
        {
            var response = await Http.Audience_DeleteRefreshV1(_ropg.AccessToken.RawData, audienceID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<AudienceModel>> Audience_GetV1(PageStateTypeC model)
        {
            var response = await Http.Audience_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<AudienceModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<AudienceModel> Audience_GetV1(string audienceValue)
        {
            var response = await Http.Audience_GetV1(_ropg.AccessToken.RawData, audienceValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<AudienceModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshModel>> Audience_GetRefreshesV1(string audienceValue)
        {
            var response = await Http.Audience_GetRefreshesV1(_ropg.AccessToken.RawData, audienceValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Audience_SetPasswordV1(Guid audienceID, PasswordAddModel model)
        {
            var response = await Http.Audience_SetPasswordV1(_ropg.AccessToken.RawData, audienceID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<AudienceModel> Audience_UpdateV1(AudienceModel model)
        {
            var response = await Http.Audience_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<AudienceModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_CreateV1(ClaimCreate model)
        {
            var response = await Http.Claim_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Claim_DeleteV1(Guid claimID)
        {
            var response = await Http.Claim_DeleteV1(_ropg.AccessToken.RawData, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_GetV1(string claimValue)
        {
            var response = await Http.Claim_GetV1(_ropg.AccessToken.RawData, claimValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<ClaimModel>> Claim_GetV1(PageStateTypeC model)
        {
            var response = await Http.Claim_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_UpdateV1(ClaimModel model)
        {
            var response = await Http.Claim_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_CreateV1(IssuerCreate model)
        {
            var response = await Http.Issuer_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Issuer_DeleteV1(Guid issuerID)
        {
            var response = await Http.Issuer_DeleteV1(_ropg.AccessToken.RawData, issuerID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<AudienceModel>> Issuer_GetAudiencesV1(string issuerValue)
        {
            var response = await Http.Issuer_GetAudiencesV1(_ropg.AccessToken.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<AudienceModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_GetV1(string issuerValue)
        {
            var response = await Http.Issuer_GetV1(_ropg.AccessToken.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<IssuerModel>> Issuer_GetV1(PageStateTypeC model)
        {
            var response = await Http.Issuer_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<IssuerModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<Dictionary<Guid, string>> Issuer_GetKeysV1(List<string> model)
        {
            var response = await Http.Issuer_GetKeysV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<Dictionary<Guid, string>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_UpdateV1(IssuerModel model)
        {
            var response = await Http.Issuer_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_CreateV1(LoginCreate model)
        {
            var response = await Http.Login_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Login_DeleteV1(Guid loginID)
        {
            var response = await Http.Login_DeleteV1(_ropg.AccessToken.RawData, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_GetV1(string loginValue)
        {
            var response = await Http.Login_GetV1(_ropg.AccessToken.RawData, loginValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<LoginModel>> Login_GetV1(PageStateTypeC model)
        {
            var response = await Http.Login_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_UpdateV1(LoginModel model)
        {
            var response = await Http.Login_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<MOTDModel> MOTD_GetV1(string motdValue)
        {
            var response = await Http.MOTD_GetV1(_ropg.AccessToken.RawData, motdValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<MOTDModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<MOTDModel>> MOTD_GetV1(PageStateTypeC model)
        {
            var response = await Http.MOTD_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<MOTDModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_CreateV1(RoleCreate model)
        {
            var response = await Http.Role_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Role_DeleteV1(Guid roleID)
        {
            var response = await Http.Role_DeleteV1(_ropg.AccessToken.RawData, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_GetV1(string roleValue)
        {
            var response = await Http.Role_GetV1(_ropg.AccessToken.RawData, roleValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<RoleModel>> Role_GetV1(PageStateTypeC model)
        {
            var response = await Http.Role_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_UpdateV1(RoleModel model)
        {
            var response = await Http.Role_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToClaimV1(Guid userID, Guid claimID)
        {
            var response = await Http.User_AddToClaimV1(_ropg.AccessToken.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToLoginV1(Guid userID, Guid loginID)
        {
            var response = await Http.User_AddToLoginV1(_ropg.AccessToken.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToRoleV1(Guid userID, Guid roleID)
        {
            var response = await Http.User_AddToRoleV1(_ropg.AccessToken.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_CreateV1(UserCreate model)
        {
            var response = await Http.User_CreateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_CreateV1NoConfirm(UserCreate model)
        {
            var response = await Http.User_CreateV1NoConfirm(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteV1(Guid userID)
        {
            var response = await Http.User_DeleteV1(_ropg.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshesV1(Guid userID)
        {
            var response = await Http.User_DeleteRefreshesV1(_ropg.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = await Http.User_DeleteRefreshV1(_ropg.AccessToken.RawData, userID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<ClaimModel>> User_GetClaimsV1(string userValue)
        {
            var response = await Http.User_GetClaimsV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<AudienceModel>> User_GetAudiencesV1(string userValue)
        {
            var response = await Http.User_GetAudiencesV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<AudienceModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<LoginModel>> User_GetLoginsV1(string userValue)
        {
            var response = await Http.User_GetLoginsV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshModel>> User_GetRefreshesV1(string userValue)
        {
            var response = await Http.User_GetRefreshesV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RoleModel>> User_GetRolesV1(string userValue)
        {
            var response = await Http.User_GetRolesV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_GetV1(string userValue)
        {
            var response = await Http.User_GetV1(_ropg.AccessToken.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<UserModel>> User_GetV1(PageStateTypeC model)
        {
            var response = await Http.User_GetV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<UserModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromClaimV1(Guid userID, Guid claimID)
        {
            var response = await Http.User_RemoveFromClaimV1(_ropg.AccessToken.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromLoginV1(Guid userID, Guid loginID)
        {
            var response = await Http.User_RemoveFromLoginV1(_ropg.AccessToken.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromRoleV1(Guid userID, Guid roleID)
        {
            var response = await Http.User_RemoveFromRoleV1(_ropg.AccessToken.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemovePasswordV1(Guid userID)
        {
            var response = await Http.User_RemovePasswordV1(_ropg.AccessToken.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_SetPasswordV1(Guid userID, PasswordAddModel model)
        {
            var response = await Http.User_SetPasswordV1(_ropg.AccessToken.RawData, userID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_UpdateV1(UserModel model)
        {
            var response = await Http.User_UpdateV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
