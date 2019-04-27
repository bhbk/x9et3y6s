using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
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
 * https://tools.ietf.org/html/draft-ietf-oauth-device-flow-15
 */

/*
 * https://oauth.net/2/grant-types/device-code/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    [AllowAnonymous]
    public class DeviceCodeController : BaseController
    {
        public DeviceCodeController() { }

        [Route("v1/dcg-ask"), HttpPost]
        public IActionResult DeviceCodeV1_Ask([FromForm] DeviceCodeAskV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrDeviceCodeIDV1, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/dcg"), HttpPost]
        public IActionResult DeviceCodeV1_Use([FromForm] DeviceCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrDeviceCodeIDV1, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/dcg-ask"), HttpPost]
        public async Task<IActionResult> DeviceCodeV2_Ask([FromForm] DeviceCodeAskV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrDeviceCodeIDV2, StringComparison.OrdinalIgnoreCase))
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

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
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

            var authorize = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorize"));
            var nonce = RandomValues.CreateBase64String(32);

            //create domain model for this result type...
            var result = new DeviceCodeV2()
            {
                issuer = issuer.Id.ToString(),
                client = client.Id.ToString(),
                verification_url = authorize.AbsoluteUri,
                user_code = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user),
                device_code = nonce,
                interval = UoW.ConfigRepo.DeviceCodePollMax,
            };

            var state = await UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = nonce,
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(UoW.ConfigRepo.DeviceCodeTokenExpire),
                });

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v2/dcg"), HttpPost]
        public async Task<IActionResult> DeviceCodeV2_Use([FromForm] DeviceCodeV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrDeviceCodeIDV2, StringComparison.OrdinalIgnoreCase))
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

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == input.client)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //check if state is valid...
            var state = (await UoW.StateRepo.GetAsync(x => x.StateValue == input.device_code
                && x.StateType == StateType.Device.ToString()
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow)).SingleOrDefault();

            if (state == null
                || state.StateConsume == true)
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"Device code:{input.device_code}");
                return BadRequest(ModelState);
            }
            //check if device is polling too frequently...
            else if (UoW.ConfigRepo.DeviceCodePollMax <= (new DateTimeOffset(state.LastPolling).Subtract(DateTime.UtcNow)).TotalSeconds)
            {
                state.LastPolling = DateTime.UtcNow;
                state.StateConsume = false;

                await UoW.StateRepo.UpdateAsync(state);
                await UoW.CommitAsync();

                ModelState.AddModelError(MessageType.StateSlowDown.ToString(), $"Device code:{input.device_code}");
                return BadRequest(ModelState);
            }

            //check if device has been approved/denied...
            if (!state.StateDecision.HasValue)
            {
                ModelState.AddModelError(MessageType.StatePending.ToString(), $"Device code:{input.device_code}");
                return BadRequest(ModelState);
            }
            else if (state.StateDecision.HasValue
                && !state.StateDecision.Value)
            {
                ModelState.AddModelError(MessageType.StateDenied.ToString(), $"Device code:{input.device_code}");
                return BadRequest(ModelState);
            }

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == state.UserId)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
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

            if (!await new TotpHelper(8, 10).ValidateAsync(user.SecurityStamp, input.user_code, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.user_code}");
                return BadRequest(ModelState);
            }

            //no reuse of state after this...
            state.LastPolling = DateTime.UtcNow;
            state.StateConsume = true;

            var dc = await JwtFactory.UserResourceOwnerV2(UoW, issuer, new List<tbl_Clients> { client }, user);
            var rt = await JwtFactory.UserRefreshV2(UoW, issuer, user);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = dc.token,
                refresh_token = rt,
                expires_in = (int)(new DateTimeOffset(dc.end).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            //adjust counter(s) for login success...
            await UoW.UserRepo.AccessSuccessAsync(user.Id);
            await UoW.StateRepo.UpdateAsync(state);
            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
