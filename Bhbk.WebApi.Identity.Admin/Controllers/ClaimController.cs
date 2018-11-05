using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("claim")]
    public class ClaimController : BaseController
    {
        public ClaimController() { }

        [Route("v1/{userID:guid}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClaimV1([FromRoute] Guid userID, [FromBody] Claim model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            var result = await UoW.CustomUserMgr.AddClaimAsync(user, model);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(model);
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

            var result = await UoW.CustomUserMgr.RemoveClaimAsync(user, claim);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
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
