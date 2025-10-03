using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.4
 * https://tools.ietf.org/html/rfc6749#section-6
 */

/*
 * https://oauth.net/2/grant-types/client-credentials/
 * https://oauth.net/2/grant-types/refresh-token/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ClientCredentialController : BaseController
    {
        [Route("v1/ccg"), HttpPost]
        [AllowAnonymous]
        public IActionResult ClientCredentialV1_Grant([FromForm] ClientCredentialV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/ccg-rt"), HttpPost]
        [AllowAnonymous]
        public IActionResult ClientCredentialV1_Refresh([FromForm] RefreshTokenV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/ccg"), HttpPost]
        [AllowAnonymous]
        public IActionResult ClientCredentialV2_Grant([FromForm] ClientCredentialV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            /* resolve identifier to guid */
            if (Guid.TryParse(input.client, out audienceID))
                audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = uow.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }
            else if (audience.IsLockedOut
                || !PBKDF2.Validate(audience.PasswordHashPBKDF2, input.client_secret))
            {
                var activity = uow.AuthActivity.Create(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    }));

                uow.AuthActivityAudiences.Create(new tbl_AuthActivityAudience
                {
                    AuthActivityId = activity.Id,
                    AudienceId = audience.Id,
                    CreatedUtc = activity.CreatedUtc,
                });

                uow.Commit();

                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var cc_claims = uow.Audiences.GenerateAccessClaims(issuer, audience);
            var cc = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, cc_claims);

            var ccActivity = uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            uow.AuthActivityAudiences.Create(new tbl_AuthActivityAudience
            {
                AuthActivityId = ccActivity.Id,
                AudienceId = audience.Id,
                CreatedUtc = ccActivity.CreatedUtc,
            });

            var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
            var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

            uow.Refreshes.Create(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    RefreshType = ConsumerType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                }));

            var rtActivity = uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            uow.AuthActivityAudiences.Create(new tbl_AuthActivityAudience
            {
                AuthActivityId = rtActivity.Id,
                AudienceId = audience.Id,
                CreatedUtc = rtActivity.CreatedUtc,
            });

            uow.Commit();

            SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                client = audience.Name,
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        [Route("v2/ccg-rt"), HttpPost]
        [AllowAnonymous]
        public IActionResult ClientCredentialV2_Refresh([FromForm] RefreshTokenV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), "Token:Missing");
                return BadRequest(ModelState);
            }

            var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == refreshToken).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, ConsumerType.Client.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return BadRequest(ModelState);
            }

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
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            /* resolve identifier to guid */
            if (Guid.TryParse(input.client, out audienceID))
                audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = uow.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }
            else if (audience.IsLockedOut)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var cc_claims = uow.Audiences.GenerateAccessClaims(issuer, audience);
            var cc = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, cc_claims);

            var rt_claims = uow.Audiences.GenerateRefreshClaims(issuer, audience);
            var rt = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, rt_claims);

            uow.Refreshes.Create(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    RefreshType = ConsumerType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                }));

            var activity = uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            uow.AuthActivityAudiences.Create(new tbl_AuthActivityAudience
            {
                AuthActivityId = activity.Id,
                AudienceId = audience.Id,
                CreatedUtc = activity.CreatedUtc,
            });

            uow.Commit();

            SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                client = audience.Name,
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime validTo)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = $"{Request.PathBase}/oauth2",
                Expires = validTo
            };
            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }
    }
}
