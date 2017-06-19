using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Controller
{
    [RoutePrefix("oauth")]
    public class OAuthController : BaseController
    {
        public OAuthController() { }

        public OAuthController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/token/{tokenID}/revoke"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> RevokeToken(Guid tokenID)
        {
            var foundToken = await UoW.UserMgmt.FindRefreshTokenByIdAsync(tokenID);

            if (foundToken == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalidToken);

            else
            {
                IdentityResult result = await UoW.UserMgmt.RemoveRefreshTokenByIdAsync(foundToken.Id);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }
    }
}
