using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IUserService
    {
        UserServiceRepository Endpoints { get; }
        IOAuth2JwtGrant Grant { get; set; }

        /*
         * profile
         */
        ValueTask<UserV1> Profile_GetV1();
        ValueTask<UserV1> Profile_UpdateV1(UserV1 model);

        /*
         * session
         */
        ValueTask<bool> Session_DeleteCodesV1();
        ValueTask<bool> Session_DeleteCodeV1(Guid codeID);
        ValueTask<bool> Session_DeleteRefreshesV1();
        ValueTask<bool> Session_DeleteRefreshV1(Guid refreshID);
        ValueTask<IEnumerable<StateV1>> Session_GetCodesV1();
        ValueTask<IEnumerable<RefreshV1>> Session_GetRefreshesV1();
        ValueTask<bool> Session_UpdateCodeV1(string codeValue, string actionValue);

        /*
         * credentials
         */
        ValueTask<bool> Credentials_SetPasswordV1(PasswordAddV1 model);

        /*
         * quote
         */
        ValueTask<QuoteV1> Quote_GetV1();
    }
}
