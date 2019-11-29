using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAdminService
    {
        JwtSecurityToken Jwt { get; set; }
        AdminRepository Http { get; }

        /*
         * activity
         */
        ValueTask<PageStateTypeCResult<ActivityModel>> Activity_GetV1(PageStateTypeC model);

        /*
         * claims
         */
        ValueTask<ClaimModel> Claim_CreateV1(ClaimCreate model);
        ValueTask<bool> Claim_DeleteV1(Guid claimID);
        ValueTask<PageStateTypeCResult<ClaimModel>> Claim_GetV1(PageStateTypeC model);
        ValueTask<ClaimModel> Claim_GetV1(string claimValue);
        ValueTask<ClaimModel> Claim_UpdateV1(ClaimModel model);

        /*
         * clients
         */
        ValueTask<ClientModel> Client_CreateV1(ClientCreate model);
        ValueTask<bool> Client_DeleteV1(Guid clientID);
        ValueTask<bool> Client_DeleteRefreshesV1(Guid clientID);
        ValueTask<bool> Client_DeleteRefreshV1(Guid clientID, Guid refreshID);
        ValueTask<PageStateTypeCResult<ClientModel>> Client_GetV1(PageStateTypeC model);
        ValueTask<ClientModel> Client_GetV1(string clientValue);
        ValueTask<IEnumerable<RefreshModel>> Client_GetRefreshesV1(string clientValue);
        ValueTask<bool> Client_SetPasswordV1(Guid clientID, EntityAddPassword model);
        ValueTask<ClientModel> Client_UpdateV1(ClientModel model);

        /*
         * issuers
         */
        ValueTask<IssuerModel> Issuer_CreateV1(IssuerCreate model);
        ValueTask<bool> Issuer_DeleteV1(Guid issuerID);
        ValueTask<IEnumerable<ClientModel>> Issuer_GetClientsV1(string issuerValue);
        ValueTask<PageStateTypeCResult<IssuerModel>> Issuer_GetV1(PageStateTypeC model);
        ValueTask<IssuerModel> Issuer_GetV1(string issuerValue);
        ValueTask<IssuerModel> Issuer_UpdateV1(IssuerModel model);

        /*
         * logins
         */
        ValueTask<LoginModel> Login_CreateV1(LoginCreate model);
        ValueTask<bool> Login_DeleteV1(Guid loginID);
        ValueTask<PageStateTypeCResult<LoginModel>> Login_GetV1(PageStateTypeC model);
        ValueTask<LoginModel> Login_GetV1(string loginValue);
        ValueTask<LoginModel> Login_UpdateV1(LoginModel model);

        /*
         * roles
         */
        ValueTask<RoleModel> Role_CreateV1(RoleCreate model);
        ValueTask<bool> Role_DeleteV1(Guid roleID);
        ValueTask<PageStateTypeCResult<RoleModel>> Role_GetV1(PageStateTypeC model);
        ValueTask<RoleModel> Role_GetV1(string roleValue);
        ValueTask<RoleModel> Role_UpdateV1(RoleModel model);

        /*
         * users
         */
        ValueTask<bool> User_AddToClaimV1(Guid userID, Guid claimID);
        ValueTask<bool> User_AddToLoginV1(Guid userID, Guid loginID);
        ValueTask<bool> User_AddToRoleV1(Guid userID, Guid roleID);
        ValueTask<UserModel> User_CreateV1(UserCreate model);
        ValueTask<UserModel> User_CreateV1NoConfirm(UserCreate model);
        ValueTask<bool> User_DeleteV1(Guid userID);
        ValueTask<bool> User_DeleteRefreshesV1(Guid userID);
        ValueTask<bool> User_DeleteRefreshV1(Guid userID, Guid refreshID);
        ValueTask<PageStateTypeCResult<UserModel>> User_GetV1(PageStateTypeC model);
        ValueTask<PageStateTypeCResult<MOTDType1Model>> User_GetMOTDsV1(PageStateTypeC model);
        ValueTask<UserModel> User_GetV1(string userValue);
        ValueTask<IEnumerable<ClientModel>> User_GetClientsV1(string userValue);
        ValueTask<IEnumerable<LoginModel>> User_GetLoginsV1(string userValue);
        ValueTask<IEnumerable<RefreshModel>> User_GetRefreshesV1(string userValue);
        ValueTask<IEnumerable<RoleModel>> User_GetRolesV1(string userValue);
        ValueTask<bool> User_RemoveFromClaimV1(Guid userID, Guid claimID);
        ValueTask<bool> User_RemoveFromLoginV1(Guid userID, Guid loginID);
        ValueTask<bool> User_RemoveFromRoleV1(Guid userID, Guid roleID);
        ValueTask<bool> User_RemovePasswordV1(Guid userID);
        ValueTask<bool> User_SetPasswordV1(Guid userID, EntityAddPassword model);
        ValueTask<UserModel> User_UpdateV1(UserModel model);
    }
}
