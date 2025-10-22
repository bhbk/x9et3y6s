using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Enums;
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
        [Route("v1/ig"), HttpGet]
        [AllowAnonymous]
        public IActionResult ImplicitV1_Grant([FromQuery] ImplicitV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/ig"), HttpGet]
        [AllowAnonymous]
        public IActionResult ImplicitV2_Grant([FromQuery] ImplicitV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /* clean out cruft from encoding */
            input.issuer = HttpUtility.UrlDecode(input.issuer);
            input.client = HttpUtility.UrlDecode(input.client);
            input.user = HttpUtility.UrlDecode(input.user);
            input.redirect_uri = HttpUtility.UrlDecode(input.redirect_uri);
            input.scope = HttpUtility.UrlDecode(input.scope);
            input.state = HttpUtility.UrlDecode(input.state);

            Guid issuerID;
            tbl_Issuer issuer;

            /* resolve identifier to guid */
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

            /* resolve identifier to guid */
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

            /* resolve identifier to guid */
            if (Guid.TryParse(input.user, out userID))
                user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            /* verify user is confirmed and not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            /* check if state is valid */
            var state = uow.States.Get(x => x.StateValue == input.state
                && x.StateType == ConsumerType.User.ToString()
                && x.ValidFrom < DateTime.UtcNow
                && x.ValidTo > DateTime.UtcNow).SingleOrDefault();

            if (state == null
                || state.StateConsume == true
                || state.UserId != user.Id)
            {
                var failActivity = uow.UserAuthActivities.Post(
                    map.Map<tbl_UserAuthActivity>(new UserAuthActivityV1()
                    {
                        UserId = user.Id,
                        LoginType = GrantFlowType.ImplicitV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    }));

                uow.AudienceAuthActivities.Post(new tbl_AudienceAuthActivity
                {
                    UserAuthActivityId = failActivity.Id,
                    AudienceId = audience.Id,
                    Created = failActivity.Created,
                });

                uow.Commit();

                ModelState.AddModelError(MessageType.StateInvalid.ToString(), $"User state:{input.state}");
                return BadRequest(ModelState);
            }

            var redirect = new Uri(input.redirect_uri);

            /* check if redirect url is defined for client, otherwise use identity ui base url */
            if (audience.tbl_Urls.Any(x => x.UrlHost == null && x.UrlPath == redirect.AbsolutePath))
            {
                redirect = new Uri(string.Format("{0}{1}{2}", conf["IdentityUserUrls:BaseUiUrl"], conf["IdentityUserUrls:BaseUiPath"], "/implicit-callback"));
            }
            else if (audience.tbl_Urls.Any(x => new Uri(x.UrlHost + x.UrlPath).AbsoluteUri == redirect.AbsoluteUri))
            {

            }
            else
            {
                ModelState.AddModelError(MessageType.UriInvalid.ToString(), $"Uri:{input.redirect_uri}");
                return BadRequest(ModelState);
            }

            /* no reuse of state after this */
            state.StateConsume = true;
            uow.States.Put(state);

            /* no refresh token in implicit flow */
            var imp_claims = uow.Users.GenerateAccessClaims(issuer, user);
            var imp = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, imp_claims);

            var impActivity = uow.UserAuthActivities.Post(
                map.Map<tbl_UserAuthActivity>(new UserAuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.ImplicitV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            uow.AudienceAuthActivities.Post(new tbl_AudienceAuthActivity
            {
                UserAuthActivityId = impActivity.Id,
                AudienceId = audience.Id,
                Created = impActivity.Created,
            });

            uow.Commit();

            var result = new Uri(redirect.AbsoluteUri + "#access_token=" + HttpUtility.UrlEncode(imp.RawData)
                + "&expires_in=" + HttpUtility.UrlEncode(((int)(new DateTimeOffset(imp.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds).ToString())
                + "&grant_type=implicit"
                + "&token_type=bearer"
                + "&state=" + HttpUtility.UrlEncode(input.state));

            return RedirectPermanent(result.AbsoluteUri);
        }
    }
}
