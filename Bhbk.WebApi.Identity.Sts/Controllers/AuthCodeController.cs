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
using System.Web;

/*
 * https://oauth.net/2/grant-types/authorization-code/
 * https://www.oauth.com/playground/authorization-code.html
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
    public class AuthCodeController : BaseController
    {
        public AuthCodeController() { }

        [Route("v1/authorization-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AskAuthCodeV1([FromQuery] AuthCodeRequestV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer_id = HttpUtility.UrlDecode(input.issuer_id);
            input.client_id = HttpUtility.UrlDecode(input.client_id);
            input.username = HttpUtility.UrlDecode(input.username);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);

            if (!input.response_type.Equals(Strings.AttrAuthCodeResponseIDV1))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.response_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorization-ask"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AskAuthCodeV2([FromQuery] AuthCodeRequestV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);

            if (!input.response_type.Equals(Strings.AttrAuthCodeResponseIDV2))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.response_type}");
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

            var authorize = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorization"));
            var redirect = new Uri(input.redirect_uri);

            //check if there is redirect url defined for client. if not then use base url for identity ui.
            if (client.TUrls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/authorization-callback"));
            }
            else if (client.TUrls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MsgType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            var create = await UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    NonceValue = RandomValues.CreateBase64String(32),
                    NonceType = StateType.User.ToString(),
                    NonceConsumed = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP),
                });

            await UoW.CommitAsync();

            var result = new Uri(authorize.AbsoluteUri + "?issuer=" + HttpUtility.UrlEncode(create.IssuerId.ToString())
                + "&client=" + HttpUtility.UrlEncode(create.ClientId.ToString())
                + "&user=" + HttpUtility.UrlEncode(create.UserId.ToString())
                + "&response_type=authorization_code"
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirect.AbsoluteUri)
                + "&nonce=" + HttpUtility.UrlEncode(create.NonceValue));

            ///*
            // * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#cookies
            // */

            //var cookie = new CookieOptions
            //{
            //    Expires = DateTime.Now.AddSeconds(UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP),
            //    IsEssential = true,
            //};

            //Response.Cookies.Append("BhbkAuthCodeScope", scope);
            //Response.Cookies.Append("BhbkAuthCodeState", state);
            //Response.Cookies.Append("BhbkAuthCodeUrl", result.AbsoluteUri);

            return Ok(result.AbsoluteUri);
        }

        [Route("v1/authorization"), HttpGet]
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
            input.nonce = HttpUtility.UrlDecode(input.nonce);

            if (!input.grant_type.Equals(Strings.AttrAuthCodeIDV1))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorization"), HttpGet]
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
            input.nonce = HttpUtility.UrlDecode(input.nonce);

            if (!input.grant_type.Equals(Strings.AttrAuthCodeIDV2))
            {
                ModelState.AddModelError(MsgType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }
            else if (!Uri.IsWellFormedUriString(input.redirect_uri, UriKind.Absolute))
            {
                ModelState.AddModelError(MsgType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
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
                        ModelState.AddModelError(MsgType.ClientNotFound.ToString(), $"Client:{input.client}");
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

            //check if client uri is valid...
            var redirect = new Uri(input.redirect_uri);

            foreach (var entry in clients)
            {
                if (!entry.TUrls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath)
                    && !entry.TUrls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
                {
                    ModelState.AddModelError(MsgType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                    return BadRequest(ModelState);
                }
            }

            //check if state is valid...
            var state = (await UoW.StateRepo.GetAsync(x => x.NonceValue == input.nonce
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow
                && x.NonceType == StateType.User.ToString())).SingleOrDefault();

            if (state == null)
            {
                ModelState.AddModelError(MsgType.StateInvalid.ToString(), $"State:{input.nonce}");
                return BadRequest(ModelState);
            }

            //check that payload can be decrypted and validated...
            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(user.SecurityStamp, input.code, user))
            {
                ModelState.AddModelError(MsgType.TokenInvalid.ToString(), $"Token:{input.code}");
                return BadRequest(ModelState);
            }

            var access = await JwtBuilder.UserResourceOwnerV2(UoW, issuer, clients, user);
            var refresh = await JwtBuilder.UserRefreshV2(UoW, issuer, user);

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

            return Ok(result);
        }
    }
}
