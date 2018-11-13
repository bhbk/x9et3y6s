using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class AuthorizationCodeController : BaseController
    {
        [Route("v1/authorization"), HttpGet]
        [AllowAnonymous]
        public IActionResult AuthCodeV1([FromQuery(Name = "issuer_id")] string issuerValue,
            [FromQuery(Name = "client_id")] string clientValue,
            [FromQuery(Name = "username")] string userValue,
            [FromQuery(Name = "redirect_uri")] string redirectUriValue,
            [FromQuery(Name = "scope")] string scopeValue)
        {
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/authorization-code"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AuthCodeRequestV1([FromQuery(Name = "issuer_id")] string issuerValue,
            [FromQuery(Name = "client_id")] string clientValue,
            [FromQuery(Name = "username")] string userValue,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "scope")] string scopeValue)
        {
            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = (await UoW.UserMgr.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserMgr.GetAsync(x => x.Email == userValue)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/authorization"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AuthCodeV2([FromQuery(Name = "issuer")] string issuerValue,
             [FromQuery(Name = "client")] string clientValue,
             [FromQuery(Name = "user")] string userValue,
             [FromQuery(Name = "redirect_uri")] string redirectUriValue,
             [FromQuery(Name = "grant_type")] string grantTypeValue,
             [FromQuery(Name = "code")] string authorizationCodeValue)
        {
            if (!grantTypeValue.Equals(Strings.AttrAuthorizeCodeIDV2))
                return BadRequest(Strings.MsgSysParamsInvalid);

            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = await UoW.UserMgr.FindByIdAsync(userID.ToString());
            else
                user = await UoW.UserMgr.FindByEmailAsync(userValue);

            if (user == null)
                return BadRequest(Strings.MsgUserInvalid);

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            //check that user is confirmed...
            //check that user is not locked...
            if (await UoW.UserMgr.IsLockedOutAsync(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
                return BadRequest(Strings.MsgUserInvalid);

            //check that payload can be decrypted and validated...
            if (!await new ProtectProvider(UoW.Situation.ToString()).ValidateAsync(user.PasswordHash, authorizationCodeValue, user))
                return BadRequest(Strings.MsgUserInvalidToken);

            var clientList = await UoW.UserMgr.GetClientsAsync(user);
            var clients = new List<AppClient>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(clientValue))
                clients = (await UoW.ClientRepo.GetAsync(x => clientList.Contains(x.Id.ToString())
                    && x.Enabled == true)).ToList();
            else
            {
                foreach (string entry in clientValue.Split(","))
                {
                    Guid clientID;
                    AppClient client;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out clientID))
                        client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
                    else
                        client = (await UoW.ClientRepo.GetAsync(x => x.Name == entry.Trim())).SingleOrDefault();

                    if (client == null)
                        return NotFound(Strings.MsgClientNotExist);

                    if (!client.Enabled
                        || !clientList.Contains(client.Id.ToString()))
                        return BadRequest(Strings.MsgClientInvalid);

                    clients.Add(client);
                }
            }

            //check that redirect url is valid...
            if (!clients.Any(x => x.AppClientUri.Any(y => y.AbsoluteUri == redirectUriValue)))
                return NotFound(Strings.MsgUriNotExist);

            var access = await JwtSecureProvider.CreateAccessTokenV2(UoW, issuer, clients, user);
            var refresh = await JwtSecureProvider.CreateRefreshTokenV2(UoW, issuer, user);

            var result = new
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                user = user.Id.ToString(),
                client = clients.Select(x => x.Id.ToString()),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            await UoW.ActivityRepo.CreateAsync(new AppActivity()
            {
                Id = Guid.NewGuid(),
                ActorId = user.Id,
                ActivityType = Enums.LoginType.GenerateAuthorizationCodeV2.ToString(),
                Created = DateTime.Now,
                Immutable = false
            });

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v2/authorization-code"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AuthCodeRequestV2([FromQuery(Name = "issuer")] string issuerValue,
            [FromQuery(Name = "client")] string clientValue,
            [FromQuery(Name = "user")] string userValue,
            [FromQuery(Name = "redirect_uri")] string redirectUriValue,
            [FromQuery(Name = "scope")] string scopeValue)
        {
            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(issuerValue, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == issuerValue)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(clientValue, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == clientValue)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = (await UoW.UserMgr.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserMgr.GetAsync(x => x.Email == userValue)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            //check that redirect url is valid...
            if (!client.AppClientUri.Any(x => x.AbsoluteUri == redirectUriValue))
                return NotFound(Strings.MsgUriNotExist);

            var state = RandomValues.CreateBase64String(32);
            var url = LinkBuilder.AuthorizationCodeRequest(Conf, issuer, user, redirectUriValue, scopeValue, state);

            /*
             * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-2.1#cookies
             * 
             * do some more stuff here...
             */

            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddSeconds(UoW.ConfigRepo.DefaultsBrowserCookieExpire),
            };

            Response.Cookies.Append("auth-code-state", state);
            Response.Cookies.Append("auth-code-url", url.AbsoluteUri);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
