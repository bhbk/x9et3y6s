using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

        public ClaimController(IIdentityContext context)
            : base(context) { }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClaim(Guid userID, Claim model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await Context.UserMgmt.AddClaimAsync(user.Id, model);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(model);
            }
        }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClaim(Guid userID, Claim claim)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);
            
            else
            {
                IdentityResult result = await Context.UserMgmt.RemoveClaimAsync(userID, claim);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpGet]
        public async Task<IActionResult> GetClaims(Guid userID)
        {
            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            return Ok(await Context.UserMgmt.GetClaimsAsync(userID));
        }
    }
}
