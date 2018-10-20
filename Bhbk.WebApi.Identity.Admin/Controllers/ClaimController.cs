using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("claim")]
    public class ClaimController : BaseController
    {
        public ClaimController() { }

        public ClaimController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/{userID:guid}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClaimV1([FromRoute] Guid userID, [FromBody] Claim model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var result = await IoC.UserMgmt.AddClaimAsync(user, model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(model);
            }
        }

        [Route("v1/{userID:guid}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClaimV1([FromRoute] Guid userID, [FromBody] Claim claim)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);
            
            else
            {
                var result = await IoC.UserMgmt.RemoveClaimAsync(user, claim);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1/{userID:guid}"), HttpGet]
        public async Task<IActionResult> GetClaimsV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            return Ok(await IoC.UserMgmt.GetClaimsAsync(user));
        }
    }
}
