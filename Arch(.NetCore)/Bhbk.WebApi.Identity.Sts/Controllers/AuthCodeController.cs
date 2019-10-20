using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
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
using System.Threading.Tasks;
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
        public async ValueTask<IActionResult> AuthCodeV2_Ask([FromQuery] AuthCodeAskV2 input)
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
                issuer = (await UoW.Issuers.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.Issuers.GetAsync(x => x.Name == input.issuer)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = (await UoW.Clients.GetAsync(x => x.Id == clientID, x => x.Include(u => u.tbl_Urls))).SingleOrDefault();
            else
                client = (await UoW.Clients.GetAsync(x => x.Name == input.client, x => x.Include(u => u.tbl_Urls))).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }

            Guid userID;
            tbl_Users user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = (await UoW.Users.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.Users.GetAsync(x => x.Email == input.user)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.Users.IsLockedOutAsync(user.Id)
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

            var expire = (await UoW.Settings.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                && x.ConfigKey == Constants.ApiSettingTotpExpire)).Single();

            var state = await UoW.States.CreateAsync(
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

            await UoW.CommitAsync();

            return RedirectPermanent(UrlHelper.GenerateAuthCodeV2(authorize, redirect, state).AbsoluteUri);
        }

        [Route("v2/acg"), HttpGet]
        public async ValueTask<IActionResult> AuthCodeV2_Auth([FromQuery] AuthCodeV2 input)
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
                issuer = (await UoW.Issuers.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.Issuers.GetAsync(x => x.Name == input.issuer)).SingleOrDefault();

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
                user = (await UoW.Users.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.Users.GetAsync(x => x.Email == input.user)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (await UoW.Users.IsLockedOutAsync(user.Id)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var clientList = await UoW.Clients.GetAsync(new QueryExpression<tbl_Clients>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());
            var clients = new List<tbl_Clients>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                clients = (await UoW.Clients.GetAsync(x => clientList.Contains(x)
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid clientID;
                    tbl_Clients client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.Clients.GetAsync(x => x.Id == clientID, y => y.Include(z => z.tbl_Urls))).SingleOrDefault();
                    else
                        client = (await UoW.Clients.GetAsync(x => x.Name == entry.Trim(), y => y.Include(z => z.tbl_Urls))).SingleOrDefault();

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
            var state = (await UoW.States.GetAsync(x => x.StateValue == input.state
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow
                && x.StateType == StateType.User.ToString())).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MessageType.StateNotFound.ToString(), $"User code:{input.state}");
                return BadRequest(ModelState);
            }

            //check that payload can be decrypted and validated...
            if (!await new ProtectHelper(UoW.InstanceType.ToString()).ValidateAsync(user.SecurityStamp, input.code, user))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.code}");
                return BadRequest(ModelState);
            }

            var rop = await JwtFactory.UserResourceOwnerV2(UoW, Mapper, issuer, clients, user);
            var rt = await JwtFactory.UserRefreshV2(UoW, Mapper, issuer, user);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                refresh_token = rt.RawData,
                user = user.Email,
                client = clients.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + UoW.Issuers.Salt,
                expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            //adjust counter(s) for login success...
            await UoW.Users.AccessSuccessAsync(user);
            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
