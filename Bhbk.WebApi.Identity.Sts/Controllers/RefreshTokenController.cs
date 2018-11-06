using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.EntityModels;
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
            ClientModel client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.client_id, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == submit.client_id)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            if (!client.Enabled)
                return BadRequest(Strings.MsgClientInvalid);

            var refreshToken = (await UoW.UserRepo.GetRefreshTokensAsync(x => x.ProtectedTicket == submit.refresh_token)).SingleOrDefault();

            if (refreshToken == null
                || refreshToken.IssuedUtc >= DateTime.UtcNow
                || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                return BadRequest(Strings.MsgUserTokenInvalid);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == refreshToken.UserId)).SingleOrDefault();

            //check that user exists...
            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is not locked...
            if (await UoW.UserRepo.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var access = await JwtProvider.CreateAccessTokenV1(UoW, issuer, client, user);
            var refresh = await JwtProvider.CreateRefreshTokenV1(UoW, issuer, user);

            var result = new JwtV1()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user_id = user.Id.ToString(),
                client_id = client.Id.ToString(),
                issuer_id = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
            {
                ActorId = user.Id,
                ActivityType = Enums.LoginType.GenerateRefreshTokenV1.ToString(),
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

            var refreshToken = (await UoW.UserRepo.GetRefreshTokensAsync(x => x.ProtectedTicket == submit.refresh_token)).SingleOrDefault();

            if (refreshToken == null
                || refreshToken.IssuedUtc >= DateTime.UtcNow
                || refreshToken.ExpiresUtc <= DateTime.UtcNow)
                return BadRequest(Strings.MsgUserTokenInvalid);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == refreshToken.UserId)).SingleOrDefault();

            //check that user exists...
            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is not locked...
            if (await UoW.UserRepo.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var clientList = await UoW.UserRepo.GetClientsAsync(user);
            var clients = new List<ClientModel>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(submit.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x.Id.ToString())
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in submit.client.Split(","))
                {
                    Guid clientID;
                    ClientModel client;

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

            var access = await JwtProvider.CreateAccessTokenV2(UoW, issuer, clients, user);
            var refresh = await JwtProvider.CreateRefreshTokenV2(UoW, issuer, user);

            var result = new JwtV2()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user = user.Id.ToString(),
                client = clients.Select(x => x.Id.ToString()).ToList(),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
            {
                ActorId = user.Id,
                ActivityType = Enums.LoginType.GenerateRefreshTokenV2.ToString(),
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v1/refresh/{userID}"), HttpGet]
        [Route("v2/refresh/{userID}"), HttpGet]
        //[Authorize(Policy = "UserPolicy")]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> GetRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var list = await UoW.UserRepo.GetRefreshTokensAsync(x => x.UserId == userID);

            var result = list.Select(x => UoW.Convert.Map<UserRefreshModel>(x));

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{refreshID}"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke/{refreshID}"), HttpDelete]
        //[Authorize(Policy = "UserPolicy")]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var refresh = (await UoW.UserRepo.GetRefreshTokensAsync((AppUserRefresh x) => x.Id == refreshID)).SingleOrDefault();

            if (refresh == null)
                return NotFound(Strings.MsgUserTokenInvalid);

            //if (user.Id == refresh.UserId)
            //    return NotFound(Strings.MsgUserTokenInvalid);

            var result = await UoW.UserRepo.RemoveRefreshTokenAsync(refresh);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/refresh/{userID}/revoke"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke"), HttpDelete]
        //[Authorize(Policy = "UserPolicy")]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RevokeRefreshTokensV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserRepo.RemoveRefreshTokensAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
