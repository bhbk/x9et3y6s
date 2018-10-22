using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
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
        [Route("v2/refresh/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = await IoC.UserMgmt.GetRefreshTokensAsync(user);

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid tokenID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var token = await IoC.UserMgmt.FindRefreshTokenByIdAsync(tokenID.ToString());

            if (token == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            var result = await IoC.UserMgmt.RemoveRefreshTokenAsync(user, token);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/refresh/{userID}/revoke"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = await IoC.UserMgmt.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/authorization-code"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AuthCodeRequestV1([FromQuery(Name = "client")] Guid clientID,
            [FromQuery(Name = "audience")] Guid audienceID,
            [FromQuery(Name = "user")] Guid userID,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "scope")] string scope)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return NotFound(BaseLib.Statics.MsgClientNotExist);

            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorization-code"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AuthCodeRequestV2([FromQuery(Name = "client")] Guid clientID,
            [FromQuery(Name = "audience")] Guid audienceID,
            [FromQuery(Name = "user")] Guid userID,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "scope")] string scope)
        {
            var client = await IoC.ClientMgmt.FindByIdAsync(clientID);

            if (client == null)
                return NotFound(BaseLib.Statics.MsgClientNotExist);

            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            //check that redirect url is valid...
            if (!audience.AppAudienceUri.Any(x => x.AbsoluteUri == redirectUri))
                return NotFound(BaseLib.Statics.MsgUriNotExist);

            var state = RandomValues.CreateBase64String(32);
            var url = LinkBuilder.AuthorizationCodeRequest(Conf, client, user, redirectUri, scope, state);

            /*
             * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#cookies
             * 
             * do some more stuff here...
             */

            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddSeconds(IoC.ConfigMgmt.Store.DefaultsBrowserCookieExpire),                 
            };

            Response.Cookies.Append("auth-code-state", state);
            Response.Cookies.Append("auth-code-url", url.AbsoluteUri);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
