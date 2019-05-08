using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

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
        public async Task<IActionResult> ClientCredentialV2_Auth([FromForm] ClientCredentialV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
            else if (!client.Enabled
                || input.client_secret != client.ClientKey)
            {
                //adjust counter(s) for login success...
                await UoW.ClientRepo.AccessFailedAsync(client.Id);
                await UoW.CommitAsync();

                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            client.ActorId = client.Id;

            var cc = await JwtFactory.ClientResourceOwnerV2(UoW, Mapper, issuer, client);
            var rt = await JwtFactory.ClientRefreshV2(UoW, Mapper, issuer, client);

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = client.Name,
                issuer = issuer.Name + ":" + UoW.IssuerRepo.Salt,
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            //adjust counter(s) for login success...
            await UoW.ClientRepo.AccessSuccessAsync(client.Id);
            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v2/ccg-rt"), HttpPost]
        public async Task<IActionResult> ClientCredentialV2_Refresh([FromForm] RefreshTokenV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refresh = (await UoW.RefreshRepo.GetAsync(x => x.RefreshValue == input.refresh_token)).SingleOrDefault();

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
            else if (!client.Enabled)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            client.ActorId = client.Id;

            var cc = await JwtFactory.ClientResourceOwnerV2(UoW, Mapper, issuer, client);
            var rt = await JwtFactory.ClientRefreshV2(UoW, Mapper, issuer, client);

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = cc.RawData,
                refresh_token = rt.RawData,
                client = client.Name,
                issuer = issuer.Name + ":" + UoW.IssuerRepo.Salt,
                expires_in = (int)(new DateTimeOffset(cc.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            //adjust counter(s) for login success...
            await UoW.ClientRepo.AccessSuccessAsync(client.Id);
            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
