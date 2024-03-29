﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Web;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.1
 */

/*
 * https://oauth.net/2/grant-types/authorization-code/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class AuthCodeController : BaseController
    {
        [Route("v1/acg-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AuthCodeV1_Ask([FromQuery] AuthCodeAskV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/acg"), HttpGet]
        [AllowAnonymous]
        public IActionResult AuthCodeV1_Grant([FromQuery] AuthCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/acg-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AuthCodeV2_Ask([FromQuery] AuthCodeAskV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = uow.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out audienceID))
                audience = uow.Audiences.Get(x => x.Id == audienceID, x => x.Include(u => u.tbl_Urls)).SingleOrDefault();
            else
                audience = uow.Audiences.Get(x => x.Name == input.client, x => x.Include(u => u.tbl_Urls)).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }

            Guid userID;
            tbl_User user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var authorize = new Uri(string.Format("{0}/{1}/{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "authorize"));
            var redirect = new Uri(input.redirect_uri);

            //check if there is redirect url defined for client. if not then use base url for identity ui.
            if (audience.tbl_Urls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}/{1}/{2}", conf["IdentityMeUrls:BaseUiUrl"], conf["IdentityMeUrls:BaseUiPath"], "authorize-callback"));
            }
            else if (audience.tbl_Urls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.TotpExpire).Single();

            var state = uow.States.Create(
                map.Map<tbl_State>(new StateV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                }));

            uow.Commit();

            return RedirectPermanent(
                UrlFactory.GenerateAuthCodeV2(authorize, redirect, issuer.Id.ToString(), audience.Id.ToString(), user.Id.ToString(), state.StateValue).AbsoluteUri);
        }

        [Route("v2/acg"), HttpGet]
        [AllowAnonymous]
        public IActionResult AuthCodeV2_Grant([FromQuery] AuthCodeV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.code = HttpUtility.UrlDecode(input.code);
            input.state = HttpUtility.UrlDecode(input.state);

            if (!Uri.IsWellFormedUriString(input.redirect_uri, UriKind.Absolute))
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = uow.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

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

            Guid userID;
            tbl_User user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var audienceList = uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());
            var audiences = new List<tbl_Audience>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                audiences = uow.Audiences.Get(x => audienceList.Contains(x)
                    && x.IsLockedOut == false).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid audienceID;
                    tbl_Audience audience;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out audienceID))
                        audience = uow.Audiences.Get(x => x.Id == audienceID, y => y.Include(z => z.tbl_Urls)).SingleOrDefault();
                    else
                        audience = uow.Audiences.Get(x => x.Name == entry.Trim(), y => y.Include(z => z.tbl_Urls)).SingleOrDefault();

                    if (audience == null)
                    {
                        ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                        return NotFound(ModelState);
                    }
                    else if (audience.IsLockedOut
                        || !audienceList.Contains(audience))
                    {
                        ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                        return BadRequest(ModelState);
                    }

                    audiences.Add(audience);
                }
            }

            if (audiences.Count == 0)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:None");
                return BadRequest(ModelState);
            }

            //check if client uri is valid...
            var redirect = new Uri(input.redirect_uri);

            foreach (var entry in audiences)
            {
                if (!entry.tbl_Urls.Where(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath).Any()
                    && !entry.tbl_Urls.Where(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri).Any())
                {
                    ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                    return BadRequest(ModelState);
                }
            }

            //check if state is valid...
            var state = uow.States.Get(x => x.StateValue == input.state
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow
                && x.StateType == ConsumerType.User.ToString()).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MessageType.StateNotFound.ToString(), $"User code:{input.state}");
                return BadRequest(ModelState);
            }

            //check that payload can be decrypted and validated...
            if (!new PasswordTokenFactory(uow.InstanceType.ToString()).Validate(user.SecurityStamp, input.code, user.Id.ToString(), user.SecurityStamp))
            {
                uow.AuthActivity.Create(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = user.Id,
                        LoginType = GrantFlowType.AuthorizationCodeV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                    }));

                uow.Commit();

                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.code}");
                return BadRequest(ModelState);
            }

            var ac_claims = uow.Users.GenerateAccessClaims(issuer, user);
            var ac = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), ac_claims);

            uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.AuthorizationCodeV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));

            var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
            var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), rt_claims);

            uow.Refreshes.Create(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));

            uow.Commit();

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = ac.RawData,
                refresh_token = rt.RawData,
                user = user.UserName,
                client = audiences.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
                expires_in = (int)(new DateTimeOffset(ac.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
