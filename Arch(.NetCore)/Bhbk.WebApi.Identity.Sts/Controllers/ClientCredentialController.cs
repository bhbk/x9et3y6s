using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
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
    [AllowAnonymous]
    public class ClientCredentialController : BaseController
    {
        private ClientCredentialProvider _provider;

        public ClientCredentialController(IConfiguration conf, IContextService instance)
        {
            _provider = new ClientCredentialProvider(conf, instance);
        }

        [Route("v1/ccg"), HttpPost]
        public IActionResult ClientCredentialV1_Grant([FromForm] ClientCredentialV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/ccg-rt"), HttpPost]
        public IActionResult ClientCredentialV1_Refresh([FromForm] RefreshTokenV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/ccg"), HttpPost]
        public IActionResult ClientCredentialV2_Grant([FromForm] ClientCredentialV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            Guid audienceID;
            tbl_Audiences audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }
            else if (!audience.Enabled
                || new ValidationHelper().ValidatePasswordHash(audience.PasswordHash, input.client_secret) == PasswordVerificationResult.Failed)
            {
                //adjust counter(s) for login failure...
                UoW.Audiences.AccessFailed(audience);
                UoW.Commit();

                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            audience.ActorId = audience.Id;

            //adjust counter(s) for login success...
            UoW.Audiences.AccessSuccess(audience);

            var cc_claims = UoW.Audiences.GenerateAccessClaims(issuer, audience);
            var cc = Auth.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audience.Name, cc_claims);

            UoW.Activities.Create(
                Mapper.Map<tbl_Activities>(new ActivityV1()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                    Immutable = false
                }));

            var rt_claims = UoW.Audiences.GenerateRefreshClaims(issuer, audience);
            var rt = Auth.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audience.Name, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activities>(new ActivityV1()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = audience.Name,
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        [Route("v2/ccg-rt"), HttpPost]
        public IActionResult ClientCredentialV2_Refresh([FromForm] RefreshTokenV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refresh = UoW.Refreshes.Get(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.RefreshValue == input.refresh_token).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, RefreshType.Client.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
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

            Guid audienceID;
            tbl_Audiences audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client}");
                return NotFound(ModelState);
            }
            else if (!audience.Enabled)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            audience.ActorId = audience.Id;

            var cc_claims = UoW.Audiences.GenerateAccessClaims(issuer, audience);
            var cc = Auth.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audience.Name, cc_claims);

            var rt_claims = UoW.Audiences.GenerateRefreshClaims(issuer, audience);
            var rt = Auth.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audience.Name, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activities>(new ActivityV1()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = audience.Name,
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
