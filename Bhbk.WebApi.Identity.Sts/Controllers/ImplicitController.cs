using Bhbk.Lib.Identity.DomainModels.Sts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Net;

/*
 * https://oauth.net/2/grant-types/implicit/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ImplicitController : BaseController
    {
        public ImplicitController() { }

        [Route("v1/implicit-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AskImplicitV1([FromQuery] ImplicitRequestV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/implicit-ask"), HttpGet]
        [AllowAnonymous]
        public IActionResult AskImplicitV2([FromQuery] ImplicitRequestV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/implicit"), HttpGet]
        [AllowAnonymous]
        public IActionResult UseImplicitV1([FromQuery] ImplicitV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/implicit"), HttpGet]
        [AllowAnonymous]
        public IActionResult UseImplicitV2([FromQuery] ImplicitV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
