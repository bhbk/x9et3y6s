using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
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

/*
 * https://oauth.net/2/grant-types/client-credentials/
 */

/*
 * https://jonhilton.net/2017/10/11/secure-your-asp.net-core-2.0-api-part-1---issuing-a-jwt/
 * https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 * https://jonhilton.net/identify-users-permissions-with-jwts-and-asp-net-core-webapi/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ClientCredentialController : BaseController
    {
        public ClientCredentialController() { }

        [Route("v1/client"), HttpPost]
        [AllowAnonymous]
        public IActionResult GenerateClientCredentialV1([FromForm] ClientCredentialV1 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!submit.grant_type.Equals(Strings.AttrClientSecretIDV1))
                return BadRequest(Strings.MsgSysParamsInvalid);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/client"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateClientCredentialV2([FromForm] ClientCredentialV2 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!submit.grant_type.Equals(Strings.AttrClientSecretIDV2))
                return BadRequest(Strings.MsgSysParamsInvalid);

            Guid issuerID;
            AppIssuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            Guid clientID;
            AppClient client;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.client, out clientID))
                client = (await UoW.ClientRepo.GetAsync(x => x.Id == clientID)).SingleOrDefault();
            else
                client = (await UoW.ClientRepo.GetAsync(x => x.Name == submit.client)).SingleOrDefault();

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            if (!client.Enabled)
                return BadRequest(Strings.MsgClientInvalid);

            if(submit.client_secret != client.ClientKey)
                return BadRequest(Strings.MsgClientInvalid);

            var access = await JwtBuilder.CreateAccessTokenV2(UoW, issuer, client);
            var refresh = await JwtBuilder.CreateRefreshTokenV2(UoW, issuer, client);

            var result = new JwtV2()
            {
                token_type = "bearer",
                access_token = access.token,
                refresh_token = refresh,
                client = new List<string> { client.Id.ToString() },
                issuer = issuer.Id.ToString() + ":" + UoW.IssuerRepo.Salt,
            };

            //add activity entry for login...
            //await UoW.ActivityRepo.CreateAsync(new ActivityCreate()
            //{
            //    ActorId = client.Id,
            //    ActivityType = LoginType.GenerateAccessTokenV2.ToString(),
            //    Immutable = false
            //});

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}
