using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        public OAuthController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/refresh/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetRefreshTokens(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = IoC.UserMgmt.GetRefreshTokensAsync(user).Result;

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshToken(Guid userID, Guid tokenID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var token = await IoC.UserMgmt.FindRefreshTokenByIdAsync(tokenID.ToString());

            if (token == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            var result = await IoC.UserMgmt.RemoveRefreshTokenAsync(user, token);

            if (!result.Succeeded)
                return GetErrorResult(result);

            else
                return NoContent();
        }

        [Route("v1/refresh/{userID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokens(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = await IoC.UserMgmt.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            else
                return NoContent();
        }
    }
}
