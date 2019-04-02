using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

/*
 * https://oauth.net/2/grant-types/password/
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
    public class AccessTokenController : BaseController
    {
        public AccessTokenController() { }

        [Route("v1/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAccessTokenV1([FromForm] AccessTokenV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //check if issuer compatibility mode enabled.
            if (!UoW.ConfigRepo.DefaultsLegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
                return NotFound();

            Guid issuerID;
            TIssuers issuer;

            if (UoW.ConfigRepo.DefaultsLegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
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
                if (Guid.TryParse(input.issuer_id, out issuerID))
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
                else
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == input.issuer_id)).SingleOrDefault();
            }

            if (issuer == null)
            {
                ModelState.AddModelError(MsgType.IssuerNotFound.ToString(), $"Issuer:{input.issuer_id}");
                return NotFound(ModelState);
            }
            else if (!issuer.Enabled)
            {
                ModelState.AddModelError(MsgType.IssuerInvalid.ToString(), $"Issuer:{input.issuer_id}");
                return BadRequest(ModelState);
            }

            Guid clientID;
            TClients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client_id, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client_id)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MsgType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            TUsers user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.username, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == input.username)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{input.username}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var loginList = await UoW.UserRepo.GetLoginsAsync(user.Id);
            var logins = await UoW.LoginRepo.GetAsync(x => loginList.Contains(x));

            //check that login provider exists...
            if (loginList == null)
            {
                ModelState.AddModelError(MsgType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                return NotFound(ModelState);
            }

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionType.Live)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin1) || x.Name.StartsWith(Strings.ApiUnitTestLogin2)).Any()
                    && UoW.Situation == ExecutionType.UnitTest))
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError(MsgType.LoginInvalid.ToString(), $"Login for user:{user.Id}");
                return BadRequest(ModelState);
            }

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            if (UoW.ConfigRepo.DefaultsLegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
            {
                var access = await JwtBuilder.UserAccessTokenV1Legacy(UoW, issuer, client, user);

                var result = new UserJwtV1Legacy()
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.GenerateAccessTokenV1Legacy.ToString(),
                    Immutable = false
                });

                await UoW.CommitAsync();

                return Ok(result);
            }
            else
            {
                var access = await JwtBuilder.UserAccessTokenV1(UoW, issuer, client, user);
                var refresh = await JwtBuilder.UserRefreshTokenV1(UoW, issuer, user);

                var result = new UserJwtV1()
                {
                    token_type = "bearer",
                    access_token = access.token,
                    refresh_token = refresh,
                    user_id = user.Id.ToString(),
                    client_id = client.Id.ToString(),
                    issuer_id = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                //add activity entry for login...
                await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.GenerateAccessTokenV1.ToString(),
                    Immutable = false
                });

                await UoW.CommitAsync();

                return Ok(result);
            }
        }

        [Route("v2/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAccessTokenV2([FromForm] AccessTokenV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!input.grant_type.Equals(Strings.AttrUserPasswordIDV2))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            TIssuers issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == input.issuer)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MsgType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }
            else if (!issuer.Enabled)
            {
                ModelState.AddModelError(MsgType.IssuerInvalid.ToString(), $"Issuer:{input.issuer}");
                return BadRequest(ModelState);
            }

            Guid userID;
            TUsers user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == input.user)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var clientList = await UoW.UserRepo.GetClientsAsync(user.Id);
            var clients = new List<TClients>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x)
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid clientID;
                    TClients client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
                    else
                        client = (await UoW.ClientRepo.GetAsync(x => x.Name == entry.Trim())).SingleOrDefault();

                    if (client == null)
                    {
                        ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (!client.Enabled
                        || !clientList.Contains(client))
                    {
                        ModelState.AddModelError(MsgType.ClientInvalid.ToString(), $"Client:{client.Id}");
                        return BadRequest(ModelState);
                    }

                    clients.Add(client);
                }
            }

            var loginList = await UoW.UserRepo.GetLoginsAsync(user.Id);
            var logins = (await UoW.LoginRepo.GetAsync(x => loginList.Contains(x))).ToList();

            //check that login provider exists...
            if (loginList == null)
            {
                ModelState.AddModelError(MsgType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                return NotFound(ModelState);
            }

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionType.Live)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin1) || x.Name.StartsWith(Strings.ApiUnitTestLogin2)).Any()
                    && UoW.Situation == ExecutionType.UnitTest))
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError(MsgType.LoginInvalid.ToString(), $"Login for user:{user.Id}");
                return BadRequest(ModelState);
            }

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            var access = await JwtBuilder.UserAccessTokenV2(UoW, issuer, clients, user);
            var refresh = await JwtBuilder.UserRefreshTokenV2(UoW, issuer, user);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user = user.Id.ToString(),
                client = clients.Select(x => x.Id.ToString()).ToList(),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
                expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
            {
                UserId = user.Id,
                ActivityType = LoginType.GenerateAccessTokenV2.ToString(),
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
