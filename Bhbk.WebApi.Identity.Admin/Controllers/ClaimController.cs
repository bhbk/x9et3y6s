using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        public ClaimController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateClaim(Guid userID, Claim model)
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

        [Route("v1/{userID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteClaim(Guid userID, Claim claim)
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

        [Route("v1/{userID}"), HttpGet]
        public async Task<IActionResult> GetClaims(Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            return Ok(await IoC.UserMgmt.GetClaimsAsync(user));
        }
    }
}
