using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult ClientCredentialV1_Auth([FromForm] ClientCredentialV1 input)
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
        public IActionResult ClientCredentialV2_Auth([FromForm] ClientCredentialV2 input)
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

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();
            else
                client = UoW.Clients.Get(x => x.Name == input.client).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled
                || !UoW.Clients.VerifyPassword(client, input.client_secret))
            {
                //adjust counter(s) for login failure...
                UoW.Clients.AccessFailed(client);
                UoW.Commit();

                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            client.ActorId = client.Id;

            //adjust counter(s) for login success...
            UoW.Clients.AccessSuccess(client);

            var cc_claims = UoW.Clients.GenerateAccessClaims(issuer, client);
            var cc = Factory.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], client.Name, cc_claims);

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientAccessTokenV2.ToString(),
                    Immutable = false
                }));

            var rt_claims = UoW.Clients.GenerateRefreshClaims(issuer, client);
            var rt = Factory.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], client.Name, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = client.Name,
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

            Guid clientID;
            tbl_Clients client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client, out clientID))
                client = UoW.Clients.Get(x => x.Id == clientID).SingleOrDefault();
            else
                client = UoW.Clients.Get(x => x.Name == input.client).SingleOrDefault();

            if (client == null)
            {
                ModelState.AddModelError(MessageType.ClientNotFound.ToString(), $"Client:{input.client}");
                return NotFound(ModelState);
            }
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            client.ActorId = client.Id;

            var cc_claims = UoW.Clients.GenerateAccessClaims(issuer, client);
            var cc = Factory.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], client.Name, cc_claims);

            var rt_claims = UoW.Clients.GenerateRefreshClaims(issuer, client);
            var rt = Factory.ClientCredential(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], client.Name, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    RefreshType = RefreshType.Client.ToString(),
                    RefreshValue = rt.RawData,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = client.Id,
                    ActivityType = LoginType.CreateClientRefreshTokenV2.ToString(),
                    Immutable = false
                }));

            UoW.Commit();

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = client.Name,
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
