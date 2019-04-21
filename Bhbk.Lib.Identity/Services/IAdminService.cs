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
        AdminRepository Repo { get; }

        /*
         * activity
         */
        Tuple<int, IEnumerable<ActivityModel>> ActivityGetV1(CascadePager model);

        /*
         * claims
         */
        ClaimModel ClaimCreateV1(ClaimCreate model);
        void ClaimDeleteV1(Guid claimID);
        ClaimModel ClaimGetV1(string claimValue);
        Tuple<int, IEnumerable<ClaimModel>> ClaimGetV1(CascadePager model);
        ClaimModel ClaimUpdateV1(ClaimModel model);

        /*
         * clients
         */
        ClientModel ClientCreateV1(ClientCreate model);
        void ClientDeleteV1(Guid clientID);
        ClientModel ClientGetV1(string clientValue);
        Tuple<int, IEnumerable<ClientModel>> ClientGetV1(CascadePager model);
        ClientModel ClientUpdateV1(ClientModel model);

        /*
         * issuers
         */
        IssuerModel IssuerCreateV1(IssuerCreate model);
        void IssuerDeleteV1(Guid issuerID);
        IssuerModel IssuerGetV1(string issuerValue);
        IEnumerable<ClientModel> IssuerGetClientsV1(string issuerValue);
        Tuple<int, IEnumerable<IssuerModel>> IssuerGetV1(CascadePager model);
        IssuerModel IssuerUpdateV1(IssuerModel model);

        /*
         * logins
         */
        LoginModel LoginCreateV1(LoginCreate model);
        void LoginDeleteV1(Guid loginID);
        LoginModel LoginGetV1(string loginValue);
        Tuple<int, IEnumerable<LoginModel>> LoginGetV1(CascadePager model);
        LoginModel LoginUpdateV1(LoginModel model);

        /*
         * roles
         */
        void RoleAddToUserV1(Guid roleID, Guid userID);
        RoleModel RoleCreateV1(RoleCreate model);
        void RoleDeleteV1(Guid roleID);
        RoleModel RoleGetV1(string roleValue);
        Tuple<int, IEnumerable<RoleModel>> RoleGetV1(CascadePager model);
        void RoleRemoveFromUserV1(Guid roleID, Guid userID);
        RoleModel RoleUpdateV1(RoleModel model);

        /*
         * users
         */
        void UserAddToLoginV1(Guid roleID, Guid userID);
        void UserAddToPasswordV1(Guid userID, UserAddPassword model);
        UserModel UserCreateV1(UserCreate model);
        UserModel UserCreateV1NoConfirm(UserCreate model);
        void UserDeleteV1(Guid userID);
        UserModel UserGetV1(string userValue);
        Tuple<int, IEnumerable<UserModel>> UserGetV1(CascadePager model);
        IEnumerable<ClientModel> UserGetClientsV1(string userValue);
        IEnumerable<LoginModel> UserGetLoginsV1(string userValue);
        IEnumerable<RoleModel> UserGetRolesV1(string userValue);
        void UserRemoveFromLoginV1(Guid roleID, Guid userID);
        void UserRemovePasswordV1(Guid userID);
        void UserSetPasswordV1(Guid userID, UserAddPassword model);
        UserModel UserUpdateV1(UserModel model);
    }
}
