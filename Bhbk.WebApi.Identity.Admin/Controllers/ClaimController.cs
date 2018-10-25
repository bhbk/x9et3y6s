using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("claim")]
    public class ClaimController : BaseController
    {
        public ClaimController() { }

        public ClaimController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1/{userID:guid}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClaimV1([FromRoute] Guid userID, [FromBody] Claim model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            else
            {
                var result = await UoW.CustomUserMgr.AddClaimAsync(user, model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                return Ok(model);
            }
        }

        [Route("v1/{userID:guid}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClaimV1([FromRoute] Guid userID, [FromBody] Claim claim)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);
            
            else
            {
                var result = await UoW.CustomUserMgr.RemoveClaimAsync(user, claim);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                return NoContent();
            }
        }

        [Route("v1/{userID:guid}"), HttpGet]
        public async Task<IActionResult> GetClaimsV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            return Ok(await UoW.CustomUserMgr.GetClaimsAsync(user));
        }
    }
}
