using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Services
{
    public interface IMeService
    {
        JwtSecurityToken Jwt { get; set; }
        MeRepository Http { get; }

        /*
         * info
         */
        bool Info_DeleteCodesV1();
        bool Info_DeleteCodeV1(Guid codeID);
        bool Info_DeleteRefreshesV1();
        bool Info_DeleteRefreshV1(Guid refreshID);
        MOTDType1Model Info_GetMOTDV1();
        IEnumerable<StateModel> Info_GetCodesV1();
        IEnumerable<RefreshModel> Info_GetRefreshesV1();
        UserModel Info_GetV1();
        bool Info_SetPasswordV1(UserAddPassword model);
        bool Info_SetTwoFactorV1(bool statusValue);
        UserModel Info_UpdateV1(UserModel model);
        bool Info_UpdateCodeV1(string codeValue, string actionValue);
    }
}
