﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;

/*
 * https://tools.ietf.org/html/draft-ietf-oauth-device-flow-15
 */

/*
 * https://oauth.net/2/grant-types/device-code/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class DeviceCodeController : BaseController
    {
        private DeviceCodeProvider _provider;

        public DeviceCodeController(IConfiguration conf, IContextService instance)
        {
            _provider = new DeviceCodeProvider(conf, instance);
        }

        [Route("v1/dcg-ask"), HttpPost]
        [AllowAnonymous]
        public IActionResult DeviceCodeV1_Ask([FromForm] DeviceCodeAskV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/dcg"), HttpPost]
        [AllowAnonymous]
        public IActionResult DeviceCodeV1_Grant([FromForm] DeviceCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/dcg-ask"), HttpPost]
        [AllowAnonymous]
        public IActionResult DeviceCodeV2_Ask([FromForm] DeviceCodeAskV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }

            Guid userID;
            tbl_User user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }

            var expire = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingTotpExpire).Single();
            var polling = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingPollingMax).Single();

            var authorize = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorize"));
            var nonce = Base64.CreateString(32);

            //create domain model for this result type...
            var result = new DeviceCodeV2()
            {
                issuer = issuer.Id.ToString(),
                client = audience.Id.ToString(),
                verification_url = authorize.AbsoluteUri,
                user_code = new TimeBasedTokenFactory(8, 10).Generate(user.SecurityStamp, user),
                device_code = nonce,
                interval = uint.Parse(polling.ConfigValue),
            };

            var state = UoW.States.Create(
                Mapper.Map<tbl_State>(new StateV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    StateValue = nonce,
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                }));

            UoW.Commit();

            return Ok(result);
        }

        [Route("v2/dcg"), HttpPost]
        [AllowAnonymous]
        public IActionResult DeviceCodeV2_Grant([FromForm] DeviceCodeV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }
            else if (!audience.IsEnabled)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var polling = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingPollingMax).Single();

            //check if state is valid...
            var state = UoW.States.Get(x => x.StateValue == input.device_code
                && x.StateType == StateType.Device.ToString()
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow).SingleOrDefault();

            if (state == null
                || state.StateConsume == true)
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"Device code:{input.device_code}");
                return BadRequest(ModelState);
            }
            //check if device is polling too frequently...
            else if (uint.Parse(polling.ConfigValue) <= (state.LastPollingUtc.Subtract(DateTime.UtcNow)).TotalSeconds)
            {
                state.LastPollingUtc = DateTime.UtcNow;
                state.StateConsume = false;

                UoW.States.Update(state);
                UoW.Commit();

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

            var user = UoW.Users.Get(x => x.Id == state.UserId).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{state.UserId}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            if (!new TimeBasedTokenFactory(8, 10).Validate(user.SecurityStamp, input.user_code, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.user_code}");
                return BadRequest(ModelState);
            }

            //no reuse of state after this...
            state.LastPollingUtc = DateTime.UtcNow;
            state.StateConsume = true;

            //adjust state...
            UoW.States.Update(state);

            //adjust counter(s) for login success...
            UoW.Users.AccessSuccess(user);

            var dc_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var dc = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, dc_claims);

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    IsDeletable = false
                }));

            var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
            var rt = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    IsDeletable = false
                }));

            UoW.Commit();

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = dc.RawData,
                refresh_token = rt.RawData,
                user = user.UserName,
                client = new List<string>() { audience.Name },
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(dc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
