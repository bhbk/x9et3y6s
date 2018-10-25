using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
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

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth")]
    public class OAuthController : BaseController
    {
        public OAuthController() { }

        public OAuthController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1/refresh/{userID}"), HttpGet]
        [Route("v2/refresh/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.GetRefreshTokensAsync(user);

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid tokenID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var token = await UoW.CustomUserMgr.FindRefreshTokenByIdAsync(tokenID.ToString());

            if (token == null)
                return BadRequest(Strings.MsgUserInvalidToken);

            var result = await UoW.CustomUserMgr.RemoveRefreshTokenAsync(user, token);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh/{userID}/revoke"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

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
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var audience = await UoW.AudienceRepo.GetAsync(audienceID);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

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
            var client = await UoW.ClientRepo.GetAsync(clientID);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var audience = await UoW.AudienceRepo.GetAsync(audienceID);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //check that redirect url is valid...
            if (!audience.AppAudienceUri.Any(x => x.AbsoluteUri == redirectUri))
                return NotFound(Strings.MsgUriNotExist);

            var state = RandomValues.CreateBase64String(32);
            var url = LinkBuilder.AuthorizationCodeRequest(Conf, client, user, redirectUri, scope, state);

            /*
             * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#cookies
             * 
             * do some more stuff here...
             */

            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddSeconds(UoW.ConfigRepo.DefaultsBrowserCookieExpire),                 
            };

            Response.Cookies.Append("auth-code-state", state);
            Response.Cookies.Append("auth-code-url", url.AbsoluteUri);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
