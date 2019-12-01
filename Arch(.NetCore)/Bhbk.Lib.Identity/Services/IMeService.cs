using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IMeService
    {
        JwtSecurityToken Jwt { get; set; }
        MeRepository Http { get; }

        /*
         * info
         */
        ValueTask<bool> Info_DeleteCodesV1();
        ValueTask<bool> Info_DeleteCodeV1(Guid codeID);
        ValueTask<bool> Info_DeleteRefreshesV1();
        ValueTask<bool> Info_DeleteRefreshV1(Guid refreshID);
        ValueTask<MOTDType1Model> Info_GetMOTDV1();
        ValueTask<IEnumerable<StateModel>> Info_GetCodesV1();
        ValueTask<IEnumerable<RefreshModel>> Info_GetRefreshesV1();
        ValueTask<UserModel> Info_GetV1();
        ValueTask<bool> Info_SetPasswordV1(PasswordAddModel model);
        ValueTask<bool> Info_SetTwoFactorV1(bool statusValue);
        ValueTask<UserModel> Info_UpdateV1(UserModel model);
        ValueTask<bool> Info_UpdateCodeV1(string codeValue, string actionValue);
    }
}
