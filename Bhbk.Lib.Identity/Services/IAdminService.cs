using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAdminService
    {
        JwtSecurityToken Jwt { get; set; }
        AdminRepository HttpClient { get; }

        /*
         * activity
         */
        Tuple<int, IEnumerable<ActivityModel>> Activity_GetV1(CascadePager model);

        /*
         * claims
         */
        ClaimModel Claim_CreateV1(ClaimCreate model);
        bool Claim_DeleteV1(Guid claimID);
        Tuple<int, IEnumerable<ClaimModel>> Claim_GetV1(CascadePager model);
        ClaimModel Claim_GetV1(string claimValue);
        ClaimModel Claim_UpdateV1(ClaimModel model);

        /*
         * clients
         */
        ClientModel Client_CreateV1(ClientCreate model);
        bool Client_DeleteV1(Guid clientID);
        bool Client_DeleteRefreshesV1(Guid clientID);
        bool Client_DeleteRefreshV1(Guid clientID, Guid refreshID);
        Tuple<int, IEnumerable<ClientModel>> Client_GetV1(CascadePager model);
        ClientModel Client_GetV1(string clientValue);
        IEnumerable<RefreshModel> Client_GetRefreshesV1(string clientValue);
        ClientModel Client_UpdateV1(ClientModel model);

        /*
         * issuers
         */
        IssuerModel Issuer_CreateV1(IssuerCreate model);
        bool Issuer_DeleteV1(Guid issuerID);
        IEnumerable<ClientModel> Issuer_GetClientsV1(string issuerValue);
        Tuple<int, IEnumerable<IssuerModel>> Issuer_GetV1(CascadePager model);
        IssuerModel Issuer_GetV1(string issuerValue);
        IssuerModel Issuer_UpdateV1(IssuerModel model);

        /*
         * logins
         */
        LoginModel Login_CreateV1(LoginCreate model);
        bool Login_DeleteV1(Guid loginID);
        Tuple<int, IEnumerable<LoginModel>> Login_GetV1(CascadePager model);
        LoginModel Login_GetV1(string loginValue);
        LoginModel Login_UpdateV1(LoginModel model);

        /*
         * roles
         */
        bool Role_AddUserV1(Guid roleID, Guid userID);
        RoleModel Role_CreateV1(RoleCreate model);
        bool Role_DeleteV1(Guid roleID);
        Tuple<int, IEnumerable<RoleModel>> Role_GetV1(CascadePager model);
        RoleModel Role_GetV1(string roleValue);
        bool Role_RemoveUserV1(Guid roleID, Guid userID);
        RoleModel Role_UpdateV1(RoleModel model);

        /*
         * users
         */
        bool User_AddLoginV1(Guid userID, Guid loginID);
        bool User_AddPasswordV1(Guid userID, UserAddPassword model);
        UserModel User_CreateV1(UserCreate model);
        UserModel User_CreateV1NoConfirm(UserCreate model);
        bool User_DeleteV1(Guid userID);
        bool User_DeleteRefreshesV1(Guid userID);
        bool User_DeleteRefreshV1(Guid userID, Guid refreshID);
        Tuple<int, IEnumerable<UserModel>> User_GetV1(CascadePager model);
        UserModel User_GetV1(string userValue);
        IEnumerable<ClientModel> User_GetClientsV1(string userValue);
        IEnumerable<LoginModel> User_GetLoginsV1(string userValue);
        IEnumerable<RefreshModel> User_GetRefreshesV1(string userValue);
        IEnumerable<RoleModel> User_GetRolesV1(string userValue);
        bool User_RemoveLoginV1(Guid roleID, Guid userID);
        bool User_RemovePasswordV1(Guid userID);
        bool User_SetPasswordV1(Guid userID, UserAddPassword model);
        UserModel User_UpdateV1(UserModel model);
    }
}
