﻿using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
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

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
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
                uow.AuthActivity.Create(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        AudienceId = audience.Id,
                        LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                    }));
                uow.Commit();

                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var cc_claims = uow.Audiences.GenerateAccessClaims(issuer, audience);
            var cc = auth.ClientCredential(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audience.Name, cc_claims);

            uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    AudienceId = audience.Id,
                    LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));

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
                }));

            uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    AudienceId = audience.Id,
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));

            uow.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
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

            var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == input.refresh_token).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, ConsumerType.Client.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
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

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
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
                }));

            uow.AuthActivity.Create(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    AudienceId = audience.Id,
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));

            uow.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = audience.Name,
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
