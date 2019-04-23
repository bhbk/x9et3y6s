using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Helpers;
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
        public AuthCodeController() { }

        [Route("v1/authorize-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AskAuthCodeV1([FromQuery] AuthCodeAskV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer_id = HttpUtility.UrlDecode(input.issuer_id);
            input.client_id = HttpUtility.UrlDecode(input.client_id);
            input.username = HttpUtility.UrlDecode(input.username);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);

            if (!string.Equals(input.response_type, Strings.AttrAuthCodeResponseIDV1, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.response_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorize-ask"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AskAuthCodeV2([FromQuery] AuthCodeAskV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);

            if (!string.Equals(input.response_type, Strings.AttrAuthCodeResponseIDV2, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.response_type}");
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

            var create = await UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(UoW.ConfigRepo.AuthCodeTotpExpire),
                });

            await UoW.CommitAsync();

            var result = new Uri(authorize.AbsoluteUri + "?issuer=" + HttpUtility.UrlEncode(create.IssuerId.ToString())
                + "&client=" + HttpUtility.UrlEncode(create.ClientId.ToString())
                + "&user=" + HttpUtility.UrlEncode(create.UserId.ToString())
                + "&response_type=code"
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirect.AbsoluteUri)
                + "&state=" + HttpUtility.UrlEncode(create.StateValue));

            return RedirectPermanent(result.AbsoluteUri);
        }

        [Route("v1/authorize"), HttpGet]
        [AllowAnonymous]
        public IActionResult UseAuthCodeV1([FromQuery] AuthCodeV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer_id = HttpUtility.UrlDecode(input.issuer_id);
            input.client_id = HttpUtility.UrlDecode(input.client_id);
            input.username = HttpUtility.UrlDecode(input.username);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.code = HttpUtility.UrlDecode(input.code);
            input.state = HttpUtility.UrlDecode(input.state);
            
            if (!string.Equals(input.grant_type, Strings.AttrAuthCodeIDV1, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorize"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseAuthCodeV2([FromQuery] AuthCodeV2 input)
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

            if (!string.Equals(input.grant_type, Strings.AttrAuthCodeIDV2, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }
            else if (!Uri.IsWellFormedUriString(input.redirect_uri, UriKind.Absolute))
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
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
                if (!entry.tbl_Urls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath)
                    && !entry.tbl_Urls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
                {
                    ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                    return BadRequest(ModelState);
                }
            }

            //check if state is valid...
            var state = (await UoW.StateRepo.GetAsync(x => x.StateValue == input.state
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

            var rop = await JwtHelper.UserResourceOwnerV2(UoW, issuer, clients, user);
            var rt = await JwtHelper.UserRefreshV2(UoW, issuer, user);

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
