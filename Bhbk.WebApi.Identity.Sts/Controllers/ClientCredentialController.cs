using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.4
 */

/*
 * https://oauth.net/2/grant-types/client-credentials/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ClientCredentialController : BaseController
    {
        public ClientCredentialController() { }

        [Route("v1/client"), HttpPost]
        [AllowAnonymous]
        public IActionResult UseClientCredentialV1([FromForm] ClientCredentialV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrClientSecretIDV1, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
                return BadRequest(ModelState);
            }

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/client"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UseClientCredentialV2([FromForm] ClientCredentialV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(input.grant_type, Strings.AttrClientSecretIDV2, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(MessageType.ParametersInvalid.ToString(), $"Grant type:{input.grant_type}");
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
            else if (!client.Enabled
                || input.client_secret != client.ClientKey)
            {
                ModelState.AddModelError(MessageType.ClientInvalid.ToString(), $"Client:{client.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as client id...
            client.ActorId = client.Id;

            var access = await JwtHelper.ClientResourceOwnerV2(UoW, issuer, client);
            var refresh = await JwtHelper.ClientRefreshV2(UoW, issuer, client);

            var result = new ClientJwtV2()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                client = client.Id.ToString(),
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
                expires_in = (int)(new DateTimeOffset(access.end).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
