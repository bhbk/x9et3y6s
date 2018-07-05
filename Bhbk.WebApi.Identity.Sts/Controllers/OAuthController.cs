using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth")]
    public class OAuthController : BaseController
    {
        public OAuthController() { }

        public OAuthController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/refresh/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = await IoC.UserMgmt.GetRefreshTokensAsync(user);

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid tokenID)
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

            return NoContent();
        }

        [Route("v1/refresh/{userID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = await IoC.UserMgmt.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }
    }
}
