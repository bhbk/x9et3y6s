using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
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
 * https://tools.ietf.org/html/rfc6749#section-4.2
 */

/*
 * https://oauth.net/2/grant-types/implicit/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ImplicitController : BaseController
    {
        public ImplicitController() { }

        [Route("v1/implicit"), HttpGet]
        [AllowAnonymous]
        public IActionResult AskImplicitV1([FromQuery] ImplicitV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //clean out cruft from encoding...
            input.issuer_id = HttpUtility.UrlDecode(input.issuer_id);
            input.client_id = HttpUtility.UrlDecode(input.client_id);
            input.username = HttpUtility.UrlDecode(input.username);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);
            input.state = HttpUtility.UrlDecode(input.state);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/implicit"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AskImplicitV2([FromQuery] ImplicitV2 input)
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

            var redirect = new Uri(input.redirect_uri);

            //check if there is redirect url defined for client. if not then use base url for identity ui.
            if (client.TUrls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}{1}{2}", Conf["IdentityMeUrls:BaseUiUrl"], Conf["IdentityMeUrls:BaseUiPath"], "/implicit-callback"));
            }
            else if (client.TUrls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MsgType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            var rop = await JwtBuilder.UserResourceOwnerV2(UoW, issuer, new List<TClients> { client }, user);

            var result = new Uri(redirect.AbsoluteUri + "#access_token=" + HttpUtility.UrlEncode(rop.token)
                + "&expires_in=" + HttpUtility.UrlEncode(((int)(new DateTimeOffset(rop.end).Subtract(DateTime.UtcNow)).TotalSeconds).ToString())
                + "&grant_type=implicit"
                + "&token_type=bearer"
                + "&state=" + HttpUtility.UrlEncode(input.state));

            return RedirectPermanent(result.AbsoluteUri);
        }
    }
}
