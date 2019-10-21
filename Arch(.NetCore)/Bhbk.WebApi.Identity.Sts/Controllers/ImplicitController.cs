using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Web;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.2
 */

/*
 * https://oauth.net/2/grant-types/implicit/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    [AllowAnonymous]
    public class ImplicitController : BaseController
    {
        private ImplicitProvider _provider;

        public ImplicitController(IConfiguration conf, IContextService instance)
        {
            _provider = new ImplicitProvider(conf, instance);
        }

        [Route("v1/ig"), HttpGet]
        public IActionResult ImplicitV1_Auth([FromQuery] ImplicitV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/ig"), HttpGet]
        public IActionResult ImplicitV2_Auth([FromQuery] ImplicitV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);
            input.state = HttpUtility.UrlDecode(input.state);

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

            //check if state is valid...
            var state = UoW.States.Get(x => x.StateValue == input.state
                && x.StateType == StateType.User.ToString()
                && x.ValidFromUtc < DateTime.UtcNow
                && x.ValidToUtc > DateTime.UtcNow).SingleOrDefault();

            if (state == null
                || state.StateConsume == true
                || state.UserId != user.Id)
            {
                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"User state:{input.state}");
                return BadRequest(ModelState);
            }

            var redirect = new Uri(input.redirect_uri);

            //check if there is redirect url defined for client. if not then use base url for identity ui.
            if (client.tbl_Urls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/implicit-callback"));
            }
            else if (client.tbl_Urls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            //no reuse of state after this...
            state.StateConsume = true;
            UoW.States.Update(state);

            //adjust counter(s) for login success...
            UoW.Users.AccessSuccess(user);

            //no refresh token as part of this flow...
            var imp_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var imp = Factory.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, UoW.Issuers.Salt, client.Name, imp_claims);

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new Uri(redirect.AbsoluteUri + "#access_token=" + HttpUtility.UrlEncode(imp.RawData)
                + "&expires_in=" + HttpUtility.UrlEncode(((int)(new DateTimeOffset(imp.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds).ToString())
                + "&grant_type=implicit"
                + "&token_type=bearer"
                + "&state=" + HttpUtility.UrlEncode(input.state));

            return RedirectPermanent(result.AbsoluteUri);
        }
    }
}
