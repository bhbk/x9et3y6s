using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth")]
    public class OAuthController : BaseController
    {
        public OAuthController() { }

        public OAuthController(IIdentityContext context)
            : base(context) { }

        [Route("v1/refresh/{tokenID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeToken(Guid tokenID)
        {
            var token = await Context.UserMgmt.FindRefreshTokenByIdAsync(tokenID);

            if (token == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await Context.UserMgmt.RemoveRefreshTokenAsync(token.ClientId, token.AudienceId, token.UserId);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }
    }
}
