﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    [AllowAnonymous]
    public class AuthCodeController : BaseController
    {
        private AuthCodeProvider _provider;

        public AuthCodeController(IConfiguration conf, IContextService instance)
        {
            _provider = new AuthCodeProvider(conf, instance);
        }

        [Route("v1/acg-ask"), HttpGet]
        public IActionResult AuthCodeV1_Ask([FromQuery] AuthCodeAskV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/acg"), HttpGet]
        public IActionResult AuthCodeV1_Auth([FromQuery] AuthCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/acg-ask"), HttpGet]
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
            tbl_Issuers issuer;

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

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = UoW.Clients.Get(x => x.Id == clientID, x => x.Include(u => u.tbl_Urls)).SingleOrDefault();
            else
                client = UoW.Clients.Get(x => x.Name == input.client, x => x.Include(u => u.tbl_Urls)).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }

            Guid userID;
            tbl_Users user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.Email == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var authorize = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorize"));
            var redirect = new Uri(input.redirect_uri);

            //check if there is redirect url defined for client. if not then use base url for identity ui.
            if (client.tbl_Urls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorize-callback"));
            }
            else if (client.tbl_Urls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            var expire = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingTotpExpire).Single();

            var state = UoW.States.Create(
                Mapper.Map<tbl_States>(new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                }));

            UoW.Commit();

            return RedirectPermanent(UrlFactory.GenerateAuthCodeV2(authorize, redirect, state).AbsoluteUri);
        }

        [Route("v2/acg"), HttpGet]
        public IActionResult AuthCodeV2_Auth([FromQuery] AuthCodeV2 input)
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
            tbl_Issuers issuer;

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
            else if (!issuer.Enabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_Users user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.Email == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var clientList = UoW.Clients.Get(new QueryExpression<tbl_Clients>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());
            var clients = new List<tbl_Clients>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                clients = UoW.Clients.Get(x => clientList.Contains(x)
                    && x.Enabled == true).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid clientID;
                    tbl_Clients client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = UoW.Clients.Get(x => x.Id == clientID, y => y.Include(z => z.tbl_Urls)).SingleOrDefault();
                    else
                        client = UoW.Clients.Get(x => x.Name == entry.Trim(), y => y.Include(z => z.tbl_Urls)).SingleOrDefault();

                    if (client == null)
                    {
                        ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
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

            //check if client uri is valid...
            var redirect = new Uri(input.redirect_uri);

            foreach (var entry in clients)
            {
                if (!entry.tbl_Urls.Where(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath).Any()
                    && !entry.tbl_Urls.Where(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri).Any())
                {
                    ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                    return BadRequest(ModelState);
                }
            }

            //check if state is valid...
            var state = UoW.States.Get(x => x.StateValue == input.state
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow
                && x.StateType == StateType.User.ToString()).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MessageType.StateNotFound.ToString(), $"User code:{input.state}");
                return BadRequest(ModelState);
            }

            //check that payload can be decrypted and validated...
            if (!new PasswordlessTokenFactory(UoW.InstanceType.ToString()).Validate(user.SecurityStamp, input.code, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.code}");
                return BadRequest(ModelState);
            }

            //adjust counter(s) for login success...
            UoW.Users.AccessSuccess(user);

            var ac_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var ac = Factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, UoW.Issuers.Salt, clients.Select(x => x.Name).ToList(), ac_claims);

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false
                }));

            var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
            var rt = Factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, UoW.Issuers.Salt, clients.Select(x => x.Name).ToList(), rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = ac.RawData,
                refresh_token = rt.RawData,
                user = user.Email,
                client = clients.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + UoW.Issuers.Salt,
                expires_in = (int)(new DateTimeOffset(ac.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
