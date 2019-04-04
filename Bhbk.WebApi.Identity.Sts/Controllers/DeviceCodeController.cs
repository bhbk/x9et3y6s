using Bhbk.Lib.Core.Cryptography;
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
using System.Net;
using System.Threading.Tasks;

/*
 * https://oauth.net/2/grant-types/device-code/
 * https://www.oauth.com/playground/device-code.html
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
    public class DeviceCodeController : BaseController
    {
        public DeviceCodeController() { }

        [Route("v1/device-ask"), HttpPost]
        [AllowAnonymous]
        public IActionResult AskDeviceCodeV1([FromForm] DeviceCodeRequestV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!input.grant_type.Equals(Strings.AttrDeviceCodeIDV1))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/device-ask"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AskDeviceCodeV2([FromForm] DeviceCodeRequestV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!input.grant_type.Equals(Strings.AttrDeviceCodeIDV2))
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

            Guid clientID;
            TClients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
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

            var authorize = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorization"));
            var nonce = RandomValues.CreateBase64String(32);

            var create = await UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    NonceValue = nonce,
                    NonceType = StateType.Device.ToString(),
                    NonceConsumed = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(UoW.ConfigRepo.DefaultsExpireDeviceCodeTOTP),
                });

            await UoW.CommitAsync();

            //create domain model for this result type...
            var result = new
            {
                issuer = issuer.Id.ToString(),
                client = client.Id.ToString(),
                verification_url = authorize.AbsoluteUri,
                user_code = await new TotpProvider(8, 10).GenerateAsync(user.SecurityStamp, user),
                device_code = nonce,
                interval = 10,
            };

            return Ok(result);
        }

        [Route("v1/device"), HttpPost]
        [AllowAnonymous]
        public IActionResult UseDeviceCodeV1([FromForm] DeviceCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!input.grant_type.Equals(Strings.AttrDeviceCodeIDV1))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/device"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseDeviceCodeV2([FromForm] DeviceCodeV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!input.grant_type.Equals(Strings.AttrDeviceCodeIDV2))
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

            Guid clientID;
            TClients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MsgType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            /*
             * TODO: finish implementation...
             * https://www.oauth.com/oauth2-servers/device-flow/token-request/
             */

            //check if state is valid...
            var state = (await UoW.StateRepo.GetAsync(x => x.NonceValue == input.device_code
                && x.NonceType == StateType.Device.ToString()
                && x.NonceConsumed == false
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow)).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MsgType.StateInvalid.ToString(), $"State:{input.device_code}");
                return BadRequest(ModelState);
            }
            //check if device is polling too frequently...
            else if(state.LastPolling < DateTime.UtcNow.AddSeconds(-10))
            {
                ModelState.AddModelError(MsgType.StateSlowDown.ToString(), $"State:SlowDown");
                return BadRequest(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == state.UserId)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{user.Id}");
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

            if (!await new TotpProvider(8, 10).ValidateAsync(user.SecurityStamp, input.user_code, user))
            {
                ModelState.AddModelError(MsgType.TokenInvalid.ToString(), $"Token:{input.user_code}");
                return BadRequest(ModelState);
            }
            else
            {
                state.NonceConsumed = true;

                await UoW.StateRepo.UpdateAsync(state);
                await UoW.CommitAsync();
            }

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);

            var access = await JwtBuilder.DeviceResourceOwnerV2(UoW, issuer, new List<TClients> { client }, user);
            var refresh = await JwtBuilder.DeviceRefreshV2(UoW, issuer, user);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
