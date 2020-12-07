using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IAdminService
    {
        AdminServiceRepository Endpoints { get; }
        IOAuth2JwtGrant Grant { get; set; }

        /*
         * activity
         */
        ValueTask<DataStateV1Result<ActivityV1>> Activity_GetV1(DataStateV1 model);

        /*
         * audiences
         */
        ValueTask<bool> Audience_AddToRoleV1(Guid userID, Guid roleID);
        ValueTask<AudienceV1> Audience_CreateV1(AudienceV1 model);
        ValueTask<bool> Audience_DeleteV1(Guid audienceID);
        ValueTask<bool> Audience_DeleteRefreshesV1(Guid audienceID);
        ValueTask<bool> Audience_DeleteRefreshV1(Guid audienceID, Guid refreshID);
        ValueTask<AudienceV1> Audience_GetV1(string audienceValue);
        ValueTask<DataStateV1Result<AudienceV1>> Audience_GetV1(DataStateV1 model);
        ValueTask<IEnumerable<RefreshV1>> Audience_GetRefreshesV1(string audienceValue);
        ValueTask<IEnumerable<RoleV1>> Audience_GetRolesV1(string audienceValue);
        ValueTask<bool> Audience_RemoveFromRoleV1(Guid audienceID, Guid roleID);
        ValueTask<bool> Audience_RemovePasswordV1(Guid audienceID);
        ValueTask<bool> Audience_SetPasswordV1(Guid audienceID, PasswordAddV1 model);
        ValueTask<AudienceV1> Audience_UpdateV1(AudienceV1 model);

        /*
         * claims
         */
        ValueTask<ClaimV1> Claim_CreateV1(ClaimV1 model);
        ValueTask<bool> Claim_DeleteV1(Guid claimID);
        ValueTask<ClaimV1> Claim_GetV1(string claimValue);
        ValueTask<DataStateV1Result<ClaimV1>> Claim_GetV1(DataStateV1 model);
        ValueTask<ClaimV1> Claim_UpdateV1(ClaimV1 model);

        /*
         * issuers
         */
        ValueTask<IssuerV1> Issuer_CreateV1(IssuerV1 model);
        ValueTask<bool> Issuer_DeleteV1(Guid issuerID);
        ValueTask<IEnumerable<AudienceV1>> Issuer_GetAudiencesV1(string issuerValue);
        ValueTask<IssuerV1> Issuer_GetV1(string issuerValue);
        ValueTask<DataStateV1Result<IssuerV1>> Issuer_GetV1(DataStateV1 model);
        ValueTask<Dictionary<Guid, string>> Issuer_GetKeysV1(List<string> model);
        ValueTask<IssuerV1> Issuer_UpdateV1(IssuerV1 model);

        /*
         * logins
         */
        ValueTask<LoginV1> Login_CreateV1(LoginV1 model);
        ValueTask<bool> Login_DeleteV1(Guid loginID);
        ValueTask<LoginV1> Login_GetV1(string loginValue);
        ValueTask<DataStateV1Result<LoginV1>> Login_GetV1(DataStateV1 model);
        ValueTask<LoginV1> Login_UpdateV1(LoginV1 model);

        /*
         * message of the days
         */

        ValueTask<MOTDTssV1> MOTD_GetV1(string motdValue);
        ValueTask<DataStateV1Result<MOTDTssV1>> MOTD_GetV1(DataStateV1 model);

        /*
         * roles
         */
        ValueTask<RoleV1> Role_CreateV1(RoleV1 model);
        ValueTask<bool> Role_DeleteV1(Guid roleID);
        ValueTask<RoleV1> Role_GetV1(string roleValue);
        ValueTask<DataStateV1Result<RoleV1>> Role_GetV1(DataStateV1 model);
        ValueTask<RoleV1> Role_UpdateV1(RoleV1 model);

        /*
         * users
         */
        ValueTask<bool> User_AddToClaimV1(Guid userID, Guid claimID);
        ValueTask<bool> User_AddToLoginV1(Guid userID, Guid loginID);
        ValueTask<bool> User_AddToRoleV1(Guid userID, Guid roleID);
        ValueTask<UserV1> User_CreateV1(UserV1 model);
        ValueTask<UserV1> User_CreateV1NoConfirm(UserV1 model);
        ValueTask<bool> User_DeleteV1(Guid userID);
        ValueTask<bool> User_DeleteRefreshesV1(Guid userID);
        ValueTask<bool> User_DeleteRefreshV1(Guid userID, Guid refreshID);
        ValueTask<UserV1> User_GetV1(string userValue);
        ValueTask<DataStateV1Result<UserV1>> User_GetV1(DataStateV1 model);
        ValueTask<IEnumerable<AudienceV1>> User_GetAudiencesV1(string userValue);
        ValueTask<IEnumerable<LoginV1>> User_GetLoginsV1(string userValue);
        ValueTask<IEnumerable<RefreshV1>> User_GetRefreshesV1(string userValue);
        ValueTask<IEnumerable<RoleV1>> User_GetRolesV1(string userValue);
        ValueTask<bool> User_RemoveFromClaimV1(Guid userID, Guid claimID);
        ValueTask<bool> User_RemoveFromLoginV1(Guid userID, Guid loginID);
        ValueTask<bool> User_RemoveFromRoleV1(Guid userID, Guid roleID);
        ValueTask<bool> User_RemovePasswordV1(Guid userID);
        ValueTask<bool> User_SetPasswordV1(Guid userID, PasswordAddV1 model);
        ValueTask<UserV1> User_UpdateV1(UserV1 model);
    }
}
