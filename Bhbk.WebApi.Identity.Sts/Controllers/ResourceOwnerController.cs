using Bhbk.Lib.Core.UnitOfWork;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.3
 */

/*
 * https://oauth.net/2/grant-types/password/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ResourceOwnerController : BaseController
    {
        public ResourceOwnerController() { }

        [Route("v1/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseResourceOwnerV1([FromForm] ResourceOwnerV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //check if issuer compatibility mode enabled.
            if (!UoW.ConfigRepo.LegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:None");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuers issuer;

            if (UoW.ConfigRepo.LegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
            {
                //really gross but needed for backward compatibility. can be lame if more than one issuer.
                if (UoW.Situation == ExecutionContext.DeployedOrLocal)
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                        .Select(i => i.Value).First())).SingleOrDefault();

                else if (UoW.Situation == ExecutionContext.Testing)
                    issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).SingleOrDefault();

                else
                {
                    ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{input.issuer_id}");
                    return BadRequest(ModelState);
                }
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
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer_id}");
                return NotFound(ModelState);
            }
            else if (!issuer.Enabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client_id, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client_id)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_Users user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.username, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == input.username)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.username}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var loginList = await UoW.UserRepo.GetLoginsAsync(user.Id);
            var logins = await UoW.LoginRepo.GetAsync(x => loginList.Contains(x));

            //check that login provider exists...
            if (loginList == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                return NotFound(ModelState);
            }

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionContext.DeployedOrLocal)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin)).Any()
                    && UoW.Situation == ExecutionContext.Testing))
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError(MessageType.LoginInvalid.ToString(), $"Login for user:{user.Id}");
                return BadRequest(ModelState);
            }

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            if (UoW.ConfigRepo.LegacyModeIssuer
                && string.IsNullOrEmpty(input.issuer_id))
            {
                var access = await JwtBuilder.UserResourceOwnerV1_Legacy(UoW, issuer, client, user);

                var result = new UserJwtV1Legacy()
                {
                    token_type = "bearer",
                    access_token = access.token,
                    expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                return Ok(result);
            }
            else
            {
                var access = await JwtBuilder.UserResourceOwnerV1(UoW, issuer, client, user);
                var refresh = await JwtBuilder.UserRefreshV1(UoW, issuer, user);

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

                return Ok(result);
            }
        }

        [Route("v2/access"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseResourceOwnerV2([FromForm] ResourceOwnerV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (!string.Equals(input.grant_type, Strings.AttrResourceOwnerIDV2, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuers issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == input.issuer)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }
            else if (!issuer.Enabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_Users user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == input.user)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.UserRepo.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var clientList = await UoW.UserRepo.GetClientsAsync(user.Id);
            var clients = new List<tbl_Clients>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x)
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid clientID;
                    tbl_Clients client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
                    else
                        client = (await UoW.ClientRepo.GetAsync(x => x.Name == entry.Trim())).SingleOrDefault();

                    if (client == null)
                    {
                        ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (!client.Enabled
                        || !clientList.Contains(client))
                    {
                        ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
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
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                return NotFound(ModelState);
            }

            //check if login provider is local...
            else if ((UoW.Situation == ExecutionContext.DeployedOrLocal)
                && logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any())
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }

            //check if login provider is transient for unit/integration test...
            else if (logins.Where(x => x.Name == Strings.ApiDefaultLogin).Any()
                || (logins.Where(x => x.Name.StartsWith(Strings.ApiUnitTestLogin)).Any()
                    && UoW.Situation == ExecutionContext.Testing))
            {
                //check that password is valid...
                if (!await UoW.UserRepo.CheckPasswordAsync(user.Id, input.password))
                {
                    //adjust counter(s) for login failure...
                    await UoW.UserRepo.AccessFailedAsync(user.Id);

                    ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError(MessageType.LoginInvalid.ToString(), $"Login for user:{user.Id}");
                return BadRequest(ModelState);
            }

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            var rop = await JwtBuilder.UserResourceOwnerV2(UoW, issuer, clients, user);
            var rt = await JwtBuilder.UserRefreshV2(UoW, issuer, user);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.token,
                refresh_token = rt,
                user = user.Id.ToString(),
                client = clients.Select(x => x.Id.ToString()).ToList(),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
                expires_in = (int)(new DateTimeOffset(rop.end).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
