using Bhbk.Lib.Common.Extensions;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

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

        public PageStateResult<ActivityModel> Activity_GetV1(PageState model)
        {
            var response = Http.Activity_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<ActivityModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ActivityModel Activity_GetV1(string activityValue)
        {
            var response = Http.Activity_GetV1(_ropg.RopgV2.RawData, activityValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ActivityModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_CreateV1(ClaimCreate model)
        {
            var response = Http.Claim_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Claim_DeleteV1(Guid claimID)
        {
            var response = Http.Claim_DeleteV1(_ropg.RopgV2.RawData, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<ClaimModel> Claim_GetV1(PageState model)
        {
            var response = Http.Claim_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_GetV1(string claimValue)
        {
            var response = Http.Claim_GetV1(_ropg.RopgV2.RawData, claimValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClaimModel Claim_UpdateV1(ClaimModel model)
        {
            var response = Http.Claim_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClaimModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_CreateV1(ClientCreate model)
        {
            var response = Http.Client_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteV1(Guid clientID)
        {
            var response = Http.Client_DeleteV1(_ropg.RopgV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteRefreshesV1(Guid clientID)
        {
            var response = Http.Client_DeleteRefreshesV1(_ropg.RopgV2.RawData, clientID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Client_DeleteRefreshV1(Guid clientID, Guid refreshID)
        {
            var response = Http.Client_DeleteRefreshV1(_ropg.RopgV2.RawData, clientID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<ClientModel> Client_GetV1(PageState model)
        {
            var response = Http.Client_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_GetV1(string clientValue)
        {
            var response = Http.Client_GetV1(_ropg.RopgV2.RawData, clientValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RefreshModel> Client_GetRefreshesV1(string clientValue)
        {
            var response = Http.Client_GetRefreshesV1(_ropg.RopgV2.RawData, clientValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientModel Client_UpdateV1(ClientModel model)
        {
            var response = Http.Client_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_CreateV1(IssuerCreate model)
        {
            var response = Http.Issuer_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Issuer_DeleteV1(Guid issuerID)
        {
            var response = Http.Issuer_DeleteV1(_ropg.RopgV2.RawData, issuerID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClientModel> Issuer_GetClientsV1(string issuerValue)
        {
            var response = Http.Issuer_GetClientsV1(_ropg.RopgV2.RawData, issuerValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<IssuerModel> Issuer_GetV1(PageState model)
        {
            var response = Http.Issuer_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<IssuerModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_GetV1(string issuerValue)
        {
            var response = Http.Issuer_GetV1(_ropg.RopgV2.RawData, issuerValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IssuerModel Issuer_UpdateV1(IssuerModel model)
        {
            var response = Http.Issuer_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IssuerModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_CreateV1(LoginCreate model)
        {
            var response = Http.Login_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Login_DeleteV1(Guid loginID)
        {
            var response = Http.Login_DeleteV1(_ropg.RopgV2.RawData, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<LoginModel> Login_GetV1(PageState model)
        {
            var response = Http.Login_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_GetV1(string loginValue)
        {
            var response = Http.Login_GetV1(_ropg.RopgV2.RawData, loginValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public LoginModel Login_UpdateV1(LoginModel model)
        {
            var response = Http.Login_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<LoginModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_CreateV1(RoleCreate model)
        {
            var response = Http.Role_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool Role_DeleteV1(Guid roleID)
        {
            var response = Http.Role_DeleteV1(_ropg.RopgV2.RawData, roleID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<RoleModel> Role_GetV1(PageState model)
        {
            var response = Http.Role_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_GetV1(string roleValue)
        {
            var response = Http.Role_GetV1(_ropg.RopgV2.RawData, roleValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public RoleModel Role_UpdateV1(RoleModel model)
        {
            var response = Http.Role_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<RoleModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddToClaimV1(Guid userID, Guid claimID)
        {
            var response = Http.User_AddToClaimV1(_ropg.RopgV2.RawData, userID, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddToLoginV1(Guid userID, Guid loginID)
        {
            var response = Http.User_AddToLoginV1(_ropg.RopgV2.RawData, userID, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_AddToRoleV1(Guid userID, Guid roleID)
        {
            var response = Http.User_AddToRoleV1(_ropg.RopgV2.RawData, userID, roleID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_CreateV1(UserCreate model)
        {
            var response = Http.User_CreateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_CreateV1NoConfirm(UserCreate model)
        {
            var response = Http.User_CreateV1NoConfirm(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteV1(Guid userID)
        {
            var response = Http.User_DeleteV1(_ropg.RopgV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteRefreshesV1(Guid userID)
        {
            var response = Http.User_DeleteRefreshesV1(_ropg.RopgV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_DeleteRefreshV1(Guid userID, Guid refreshID)
        {
            var response = Http.User_DeleteRefreshV1(_ropg.RopgV2.RawData, userID, refreshID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<MOTDType1Model> User_GetMOTDsV1(PageState model)
        {
            var response = Http.User_GetMOTDsV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<MOTDType1Model>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClaimModel> User_GetClaimsV1(string userValue)
        {
            var response = Http.User_GetClaimsV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClaimModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<ClientModel> User_GetClientsV1(string userValue)
        {
            var response = Http.User_GetClientsV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<ClientModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<LoginModel> User_GetLoginsV1(string userValue)
        {
            var response = Http.User_GetLoginsV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<LoginModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RefreshModel> User_GetRefreshesV1(string userValue)
        {
            var response = Http.User_GetRefreshesV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RefreshModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public IEnumerable<RoleModel> User_GetRolesV1(string userValue)
        {
            var response = Http.User_GetRolesV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<IEnumerable<RoleModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public PageStateResult<UserModel> User_GetV1(PageState model)
        {
            var response = Http.User_GetV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<PageStateResult<UserModel>>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_GetV1(string userValue)
        {
            var response = Http.User_GetV1(_ropg.RopgV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemoveFromClaimV1(Guid userID, Guid claimID)
        {
            var response = Http.User_RemoveFromClaimV1(_ropg.RopgV2.RawData, userID, claimID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemoveFromLoginV1(Guid userID, Guid loginID)
        {
            var response = Http.User_RemoveFromLoginV1(_ropg.RopgV2.RawData, userID, loginID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemoveFromRoleV1(Guid userID, Guid roleID)
        {
            var response = Http.User_RemoveFromRoleV1(_ropg.RopgV2.RawData, userID, roleID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_RemovePasswordV1(Guid userID)
        {
            var response = Http.User_RemovePasswordV1(_ropg.RopgV2.RawData, userID).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public bool User_SetPasswordV1(Guid userID, UserAddPassword model)
        {
            var response = Http.User_SetPasswordV1(_ropg.RopgV2.RawData, userID, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserModel User_UpdateV1(UserModel model)
        {
            var response = Http.User_UpdateV1(_ropg.RopgV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserModel>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
