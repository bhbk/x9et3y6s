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
        private readonly ResourceOwnerGrant _ropg;
        private readonly AdminRepository _http;

        public AdminService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AdminService(IConfiguration conf, InstanceContext instance, HttpClient client)
        {
            _ropg = new ResourceOwnerGrant(instance, client);
            _http = new AdminRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _ropg.RopgV2; }
            set { _ropg.RopgV2 = value; }
        }

        public AdminRepository Http
        {
            get { return _http; }
        }

        public async ValueTask<PageStateTypeCResult<ActivityModel>> Activity_GetV1(PageStateTypeC model)
        {
            var response = await Http.Activity_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<ActivityModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ActivityModel> Activity_GetV1(string activityValue)
        {
            var response = await Http.Activity_GetV1(_ropg.RopgV2.RawData, activityValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ActivityModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_CreateV1(ClaimCreate model)
        {
            var response = await Http.Claim_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Claim_DeleteV1(Guid claimID)
        {
            var response = await Http.Claim_DeleteV1(_ropg.RopgV2.RawData, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<ClaimModel>> Claim_GetV1(PageStateTypeC model)
        {
            var response = await Http.Claim_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_GetV1(string claimValue)
        {
            var response = await Http.Claim_GetV1(_ropg.RopgV2.RawData, claimValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClaimModel> Claim_UpdateV1(ClaimModel model)
        {
            var response = await Http.Claim_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientModel> Client_CreateV1(ClientCreate model)
        {
            var response = await Http.Client_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Client_DeleteV1(Guid clientID)
        {
            var response = await Http.Client_DeleteV1(_ropg.RopgV2.RawData, clientID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Client_DeleteRefreshesV1(Guid clientID)
        {
            var response = await Http.Client_DeleteRefreshesV1(_ropg.RopgV2.RawData, clientID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Client_DeleteRefreshV1(Guid clientID, Guid refreshID)
        {
            var response = await Http.Client_DeleteRefreshV1(_ropg.RopgV2.RawData, clientID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<ClientModel>> Client_GetV1(PageStateTypeC model)
        {
            var response = await Http.Client_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientModel> Client_GetV1(string clientValue)
        {
            var response = await Http.Client_GetV1(_ropg.RopgV2.RawData, clientValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshModel>> Client_GetRefreshesV1(string clientValue)
        {
            var response = await Http.Client_GetRefreshesV1(_ropg.RopgV2.RawData, clientValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Client_SetPasswordV1(Guid clientID, EntityAddPassword model)
        {
            var response = await Http.Client_SetPasswordV1(_ropg.RopgV2.RawData, clientID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientModel> Client_UpdateV1(ClientModel model)
        {
            var response = await Http.Client_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_CreateV1(IssuerCreate model)
        {
            var response = await Http.Issuer_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Issuer_DeleteV1(Guid issuerID)
        {
            var response = await Http.Issuer_DeleteV1(_ropg.RopgV2.RawData, issuerID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<ClientModel>> Issuer_GetClientsV1(string issuerValue)
        {
            var response = await Http.Issuer_GetClientsV1(_ropg.RopgV2.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<IssuerModel>> Issuer_GetV1(PageStateTypeC model)
        {
            var response = await Http.Issuer_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<IssuerModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_GetV1(string issuerValue)
        {
            var response = await Http.Issuer_GetV1(_ropg.RopgV2.RawData, issuerValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IssuerModel> Issuer_UpdateV1(IssuerModel model)
        {
            var response = await Http.Issuer_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_CreateV1(LoginCreate model)
        {
            var response = await Http.Login_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Login_DeleteV1(Guid loginID)
        {
            var response = await Http.Login_DeleteV1(_ropg.RopgV2.RawData, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<LoginModel>> Login_GetV1(PageStateTypeC model)
        {
            var response = await Http.Login_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_GetV1(string loginValue)
        {
            var response = await Http.Login_GetV1(_ropg.RopgV2.RawData, loginValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<LoginModel> Login_UpdateV1(LoginModel model)
        {
            var response = await Http.Login_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_CreateV1(RoleCreate model)
        {
            var response = await Http.Role_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Role_DeleteV1(Guid roleID)
        {
            var response = await Http.Role_DeleteV1(_ropg.RopgV2.RawData, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<RoleModel>> Role_GetV1(PageStateTypeC model)
        {
            var response = await Http.Role_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_GetV1(string roleValue)
        {
            var response = await Http.Role_GetV1(_ropg.RopgV2.RawData, roleValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<RoleModel> Role_UpdateV1(RoleModel model)
        {
            var response = await Http.Role_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToClaimV1(Guid userID, Guid claimID)
        {
            var response = await Http.User_AddToClaimV1(_ropg.RopgV2.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToLoginV1(Guid userID, Guid loginID)
        {
            var response = await Http.User_AddToLoginV1(_ropg.RopgV2.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_AddToRoleV1(Guid userID, Guid roleID)
        {
            var response = await Http.User_AddToRoleV1(_ropg.RopgV2.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_CreateV1(UserCreate model)
        {
            var response = await Http.User_CreateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_CreateV1NoConfirm(UserCreate model)
        {
            var response = await Http.User_CreateV1NoConfirm(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteV1(Guid userID)
        {
            var response = await Http.User_DeleteV1(_ropg.RopgV2.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshesV1(Guid userID)
        {
            var response = await Http.User_DeleteRefreshesV1(_ropg.RopgV2.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = await Http.User_DeleteRefreshV1(_ropg.RopgV2.RawData, userID, refreshID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<MOTDType1Model>> User_GetMOTDsV1(PageStateTypeC model)
        {
            var response = await Http.User_GetMOTDsV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<MOTDType1Model>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<ClaimModel>> User_GetClaimsV1(string userValue)
        {
            var response = await Http.User_GetClaimsV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<ClientModel>> User_GetClientsV1(string userValue)
        {
            var response = await Http.User_GetClientsV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<LoginModel>> User_GetLoginsV1(string userValue)
        {
            var response = await Http.User_GetLoginsV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RefreshModel>> User_GetRefreshesV1(string userValue)
        {
            var response = await Http.User_GetRefreshesV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<IEnumerable<RoleModel>> User_GetRolesV1(string userValue)
        {
            var response = await Http.User_GetRolesV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<PageStateTypeCResult<UserModel>> User_GetV1(PageStateTypeC model)
        {
            var response = await Http.User_GetV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<PageStateTypeCResult<UserModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_GetV1(string userValue)
        {
            var response = await Http.User_GetV1(_ropg.RopgV2.RawData, userValue);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromClaimV1(Guid userID, Guid claimID)
        {
            var response = await Http.User_RemoveFromClaimV1(_ropg.RopgV2.RawData, userID, claimID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromLoginV1(Guid userID, Guid loginID)
        {
            var response = await Http.User_RemoveFromLoginV1(_ropg.RopgV2.RawData, userID, loginID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemoveFromRoleV1(Guid userID, Guid roleID)
        {
            var response = await Http.User_RemoveFromRoleV1(_ropg.RopgV2.RawData, userID, roleID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_RemovePasswordV1(Guid userID)
        {
            var response = await Http.User_RemovePasswordV1(_ropg.RopgV2.RawData, userID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> User_SetPasswordV1(Guid userID, EntityAddPassword model)
        {
            var response = await Http.User_SetPasswordV1(_ropg.RopgV2.RawData, userID, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserModel> User_UpdateV1(UserModel model)
        {
            var response = await Http.User_UpdateV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
