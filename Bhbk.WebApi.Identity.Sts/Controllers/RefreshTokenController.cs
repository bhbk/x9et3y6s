using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;

/*
 * https://tools.ietf.org/html/rfc6749#section-6
 */

/*
 * https://oauth.net/2/grant-types/refresh-token/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize(Policy = "AdministratorsPolicy")]
    //[Authorize(Policy = "UsersPolicy")]
    [Route("oauth2")]
    public class RefreshTokenController : BaseController
    {
        public RefreshTokenController() { }

        [Route("v1/rt/{userID}"), HttpGet]
        public IActionResult RefreshTokenV1_GetAll([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/rt/{userID}/revoke/{refreshID}"), HttpDelete]
        public IActionResult RefreshTokenV1_Revoke([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v1/rt/{userID}/revoke"), HttpDelete]
        public IActionResult RefreshTokenV1_RevokeAll([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return StatusCode((int)HttpStatusCode.NotImplemented);
        }

        [Route("v2/rt/{userID}"), HttpGet]
        public async Task<IActionResult> RefreshTokenV2_GetAll([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var tokens = await UoW.RefreshRepo.GetAsync(x => x.UserId == userID);

            var result = tokens.Select(x => UoW.Mapper.Map<RefreshModel>(x));

            return Ok(result);
        }

        [Route("v2/rt/{userID}/revoke/{refreshID}"), HttpDelete]
        public async Task<IActionResult> RefreshTokenV2_Revoke([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var token = (await UoW.RefreshRepo.GetAsync(x => x.UserId == userID
                && x.Id == refreshID)).SingleOrDefault();

            if (token == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v2/rt/{userID}/revoke"), HttpDelete]
        public async Task<IActionResult> RefreshTokenV2_RevokeAll([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            foreach (var token in await UoW.RefreshRepo.GetAsync(x => x.UserId == userID))
            {
                if (!await UoW.RefreshRepo.DeleteAsync(token.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }
    }
}
