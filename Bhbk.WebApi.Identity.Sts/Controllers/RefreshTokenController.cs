using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class RefreshTokenController : BaseController
    {
        public RefreshTokenController() { }

        [Route("v1/refresh/{userID}"), HttpGet]
        [Route("v2/refresh/{userID}"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = await UoW.UserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserMgr.GetRefreshTokensAsync(user);

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke/{tokenID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid tokenID)
        {
            var user = await UoW.UserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var token = await UoW.UserMgr.FindRefreshTokenByIdAsync(tokenID.ToString());

            if (token == null)
                return BadRequest(Strings.MsgUserInvalidToken);

            var result = await UoW.UserMgr.RemoveRefreshTokenAsync(user, token);

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
            var user = await UoW.UserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserMgr.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateRefreshTokenV1([FromForm] RefreshTokenV1 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.issuer_id, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer_id)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.client_id, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == submit.client_id)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            if (!client.Enabled)
                return BadRequest(Strings.MsgClientInvalid);

            var refreshToken = await UoW.UserMgr.FindRefreshTokenAsync(submit.refresh_token);

            if (refreshToken == null
                || refreshToken.IssuedUtc >= DateTime.UtcNow
                || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                return BadRequest(Strings.MsgUserInvalidToken);

            var user = await UoW.UserMgr.FindByIdAsync(refreshToken.UserId.ToString());

            //check that user exists...
            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is not locked...
            if (await UoW.UserMgr.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var access = await JwtSecureProvider.CreateAccessTokenV1(UoW, issuer, client, user);
            var refresh = await JwtSecureProvider.CreateRefreshTokenV1(UoW, issuer, user);

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user_id = user.Id.ToString(),
                client_id = client.Id.ToString(),
                issuer_id = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new AppActivity()
            {
                Id = Guid.NewGuid(),
                ActorId = user.Id,
                ActivityType = Enums.LoginType.GenerateRefreshTokenV1.ToString(),
                Created = DateTime.Now,
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v2/refresh"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateRefreshTokenV2([FromForm] RefreshTokenV2 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            var refreshToken = await UoW.UserMgr.FindRefreshTokenAsync(submit.refresh_token);

            if (refreshToken == null
                || refreshToken.IssuedUtc >= DateTime.UtcNow
                || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                return BadRequest(Strings.MsgUserInvalidToken);

            var user = await UoW.UserMgr.FindByIdAsync(refreshToken.UserId.ToString());

            //check that user exists...
            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is not locked...
            if (await UoW.UserMgr.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var clientList = await UoW.UserMgr.GetClientsAsync(user);
            var clients = new List<AppClient>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(submit.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x.Id.ToString())
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in submit.client.Split(","))
                {
                    Guid clientID;
                    AppClient client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
                    else
                        client = (await UoW.ClientRepo.GetAsync(x => x.Name == entry.Trim())).SingleOrDefault();

                    if (client == null)
                        return NotFound(Strings.MsgClientNotExist);

                    if (!client.Enabled
                        || !clientList.Contains(client.Id.ToString()))
                        return BadRequest(Strings.MsgClientInvalid);

                    clients.Add(client);
                }
            }

            var access = await JwtSecureProvider.CreateAccessTokenV2(UoW, issuer, clients, user);
            var refresh = await JwtSecureProvider.CreateRefreshTokenV2(UoW, issuer, user);

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user = user.Id.ToString(),
                client = clients.Select(x => x.Id.ToString()),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new AppActivity()
            {
                Id = Guid.NewGuid(),
                ActorId = user.Id,
                ActivityType = Enums.LoginType.GenerateRefreshTokenV2.ToString(),
                Created = DateTime.Now,
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
