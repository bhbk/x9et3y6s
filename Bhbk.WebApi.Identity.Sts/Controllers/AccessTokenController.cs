using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
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
        public AccessTokenController() { }

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
                if (UoW.Situation == ExecutionType.Live)
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                        .Select(i => i.Value).First())).SingleOrDefault();
                else
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).SingleOrDefault();
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
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == submit.username)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is confirmed...
            //check that user is not locked...
            if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var loginList = await UoW.UserRepo.GetLoginsAsync(user.Id);
            var logins = await UoW.LoginRepo.GetAsync(x => loginList.Contains(x));

            //check that login provider exists...
            if (loginList == null)
                return NotFound(Strings.MsgLoginNotExist);

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionType.Live)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (UoW.Situation == ExecutionType.UnitTest
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin
                    || x.Name.Contains(Strings.ApiUnitTestLogin1) || x.Name.Contains(Strings.ApiUnitTestLogin2)).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }
            else
                return BadRequest(Strings.MsgLoginInvalid);

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            if (UoW.ConfigRepo.DefaultsCompatibilityModeIssuer
                && string.IsNullOrEmpty(submit.issuer_id))
            {
                var access = await JwtBuilder.CreateAccessTokenV1CompatibilityMode(UoW, issuer, client, user);

                var result = new JwtV1Legacy()
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    ActorId = user.Id,
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1CompatibilityMode.ToString(),
                    Immutable = false
                });

                await UoW.CommitAsync();

                return Ok(result);
            }
            else
            {
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
                    ActivityType = Enums.LoginType.GenerateAccessTokenV1.ToString(),
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
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == submit.user)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is confirmed...
            //check that user is not locked...
            if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            var clientList = await UoW.UserRepo.GetClientsAsync(user.Id);
            var clients = new List<AppClient>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(submit.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x)
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
                        || !clientList.Contains(client))
                        return BadRequest(Strings.MsgClientInvalid);

                    clients.Add(client);
                }
            }

            var loginList = await UoW.UserRepo.GetLoginsAsync(user.Id);
            var logins = (await UoW.LoginRepo.GetAsync(x => loginList.Contains(x))).ToList();

            //check that login provider exists...
            if (loginList == null)
                return NotFound(Strings.MsgLoginNotExist);

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionType.Live)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (UoW.Situation == ExecutionType.UnitTest
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin
                    || x.Name.Contains(Strings.ApiUnitTestLogin1) || x.Name.Contains(Strings.ApiUnitTestLogin2)).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, submit.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    return BadRequest(Strings.MsgUserInvalid);
                }
            }
            else
                return BadRequest(Strings.MsgLoginInvalid);

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

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
                ActivityType = Enums.LoginType.GenerateAccessTokenV2.ToString(),
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
