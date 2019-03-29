using Bhbk.Lib.Identity.DomainModels.Sts;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

/*
 * https://oauth.net/2/grant-types/device-code/
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
    public class DeviceCodeController : BaseController
    {
        public DeviceCodeController() { }

        [Route("v1/device"), HttpPost]
        [AllowAnonymous]
        public IActionResult GenerateDeviceCodeV1([FromForm] DeviceCodeV1 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!submit.grant_type.Equals(Strings.AttrDeviceCodeIDV1))
                return BadRequest(Strings.MsgSysParamsInvalid);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/device"), HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateDeviceCodeV2([FromForm] DeviceCodeV2 submit)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!submit.grant_type.Equals(Strings.AttrDeviceCodeIDV2))
                return BadRequest(Strings.MsgSysParamsInvalid);

            //provider not implemented yet...
            return StatusCode((int)HttpStatusCode.NotImplemented);
        }
    }
}
