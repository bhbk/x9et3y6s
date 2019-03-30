﻿using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

/*
 * https://oauth.net/2/grant-types/refresh-token/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class RefreshTokenController : BaseController
    {
        public RefreshTokenController() { }

        [Route("v1/refresh"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseRefreshTokenV1([FromForm] RefreshTokenV1 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            TIssuers issuer;

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
            TClients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.client_id, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == submit.client_id)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            if (!client.Enabled)
                return BadRequest(Strings.MsgClientInvalid);

            var refreshToken = (await UoW.RefreshRepo.GetAsync(x => x.ProtectedTicket == submit.refresh_token)).SingleOrDefault();

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
            if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var access = await JwtBuilder.CreateAccessTokenV1(UoW, issuer, client, user);
            var refresh = await JwtBuilder.CreateRefreshTokenV1(UoW, issuer, user);

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
                ActivityType = LoginType.GenerateRefreshTokenV1.ToString(),
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v2/refresh"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseRefreshTokenV2([FromForm] RefreshTokenV2 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            TIssuers issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            var refreshToken = (await UoW.RefreshRepo.GetAsync(x => x.ProtectedTicket == submit.refresh_token)).SingleOrDefault();

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
            if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var clientList = await UoW.UserRepo.GetClientsAsync(user.Id);
            var clients = new List<TClients>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(submit.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x)
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in submit.client.Split(","))
                {
                    Guid clientID;
                    TClients client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
                    else
                        client = (await UoW.ClientRepo.GetAsync(x => x.Name == entry.Trim())).SingleOrDefault();

                    if (client == null)
                        return NotFound(Strings.MsgClientNotExist);

                    if (!client.Enabled
                        || !clientList.Contains(client))
                        return BadRequest(Strings.MsgClientInvalid);

                    clients.Add(client);
                }
            }

            var access = await JwtBuilder.CreateAccessTokenV2(UoW, issuer, clients, user);
            var refresh = await JwtBuilder.CreateRefreshTokenV2(UoW, issuer, user);

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
                ActivityType = LoginType.GenerateRefreshTokenV2.ToString(),
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

            var tokens = await UoW.RefreshRepo.GetAsync(x => x.UserId == userID);

            var result = tokens.Select(x => UoW.Transform.Map<RefreshModel>(x));

            return Ok(result);
        }

        [Route("v1/refresh/{userID}/revoke/{refreshID}"), HttpDelete]
        [Route("v2/refresh/{userID}/revoke/{refreshID}"), HttpDelete]
        //[Authorize(Policy = "UserPolicy")]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RevokeRefreshTokenV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var token = (await UoW.RefreshRepo.GetAsync(x => x.UserId == userID 
                && x.Id == refreshID)).SingleOrDefault();

            if (token == null)
                return NotFound(Strings.MsgUserTokenInvalid);

            if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

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

            foreach (var token in await UoW.RefreshRepo.GetAsync(x => x.UserId == userID))
            {
                if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
