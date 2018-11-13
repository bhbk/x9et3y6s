using Bhbk.Lib.Core.Primitives.Enums;
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
    public class AccessTokenController : BaseController
    {
        [Route("v1/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateAccessTokenV1([FromForm] AccessTokenV1 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //check if issuer compatibility mode enabled.
            if (!UoW.ConfigRepo.DefaultsCompatibilityModeIssuer
                && string.IsNullOrEmpty(submit.issuer_id))
                return NotFound();

            Guid issuerID;
            AppIssuer issuer;

            if (UoW.ConfigRepo.DefaultsCompatibilityModeIssuer
                && string.IsNullOrEmpty(submit.issuer_id))
            {
                //really gross but needed for backward compatibility. can be lame if more than one issuer.
                issuer = (await UoW.IssuerRepo.GetAsync()).First();
            }
            else
            {
                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(submit.issuer_id, out issuerID))
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
                else
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer_id)).SingleOrDefault();
            }

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

            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.username, out userID))
                user = await UoW.UserMgr.FindByIdAsync(userID.ToString());
            else
                user = await UoW.UserMgr.FindByEmailAsync(submit.username);

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is confirmed...
            //check that user is not locked...
            if (await UoW.UserMgr.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var loginList = await UoW.UserMgr.GetLoginsAsync(user);
            var logins = await UoW.LoginRepo.GetAsync(x => loginList.Contains(x.Id.ToString()));

            //check that login provider exists...
            if (loginList == null)
                return NotFound(Strings.MsgLoginNotExist);

            //check if login provider is local...
            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.LoginProvider == Strings.ApiDefaultLogin).Any()
                || (logins.Where(x => x.LoginProvider.StartsWith(Strings.ApiUnitTestLogin1)).Any() && UoW.Situation == ContextType.UnitTest))
            {
                //check that password is valid...
                if (!await UoW.UserMgr.CheckPasswordAsync(user, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserMgr.AccessFailedAsync(user);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }
            else
                return BadRequest(Strings.MsgLoginInvalid);

            //adjust counter(s) for login success...
            await UoW.UserMgr.AccessSuccessAsync(user);

            if (UoW.ConfigRepo.DefaultsCompatibilityModeIssuer
                && string.IsNullOrEmpty(submit.issuer_id))
            {
                var access = await JwtSecureProvider.CreateAccessTokenV1CompatibilityMode(UoW, issuer, client, user);

                var result = new
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                await UoW.ActivityRepo.CreateAsync(new AppActivity()
                {
                    Id = Guid.NewGuid(),
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1CompatibilityMode.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                await UoW.CommitAsync();

                return Ok(result);
            }
            else
            {
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
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1.ToString(),
                    Created = DateTime.Now,
                    Immutable = false
                });

                await UoW.CommitAsync();

                return Ok(result);
            }
        }

        [Route("v2/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateAccessTokenV2([FromForm] AccessTokenV2 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!submit.grant_type.Equals(Strings.AttrUserPasswordIDV2))
                return BadRequest(Strings.MsgSysParamsInvalid);

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

            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.user, out userID))
                user = await UoW.UserMgr.FindByIdAsync(userID.ToString());
            else
                user = await UoW.UserMgr.FindByEmailAsync(submit.user);

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is confirmed...
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

            var loginList = await UoW.UserMgr.GetLoginsAsync(user);
            var logins = (await UoW.LoginRepo.GetAsync(x => loginList.Contains(x.Id.ToString()))).ToList();

            //check that login provider exists...
            if (loginList == null)
                return NotFound(Strings.MsgLoginNotExist);

            //check if login provider is local...
            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.LoginProvider == Strings.ApiDefaultLogin).Any()
                || (UoW.Situation == ContextType.UnitTest && logins.Where(x => x.LoginProvider.StartsWith(Strings.ApiUnitTestLogin1)).Any()))
            {
                //check that password is valid...
                if (!await UoW.UserMgr.CheckPasswordAsync(user, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserMgr.AccessFailedAsync(user);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }
            else
                return BadRequest(Strings.MsgLoginInvalid);

            //adjust counter(s) for login success...
            await UoW.UserMgr.AccessSuccessAsync(user);

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
                ActivityType = Enums.LoginType.GenerateAccessTokenV2.ToString(),
                Created = DateTime.Now,
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
