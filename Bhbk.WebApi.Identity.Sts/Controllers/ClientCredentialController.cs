using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

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
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/client"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateClientCredentialV2([FromForm] ClientCredentialV2 submit)
        {
            Guid issuerID;
            IssuerModel issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(submit.issuer, out issuerID))
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == issuerID)).SingleOrDefault();
            else
                issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == submit.issuer)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            if (!issuer.Enabled)
                return BadRequest(Strings.MsgIssuerInvalid);

            //provider not implemented yet...
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
