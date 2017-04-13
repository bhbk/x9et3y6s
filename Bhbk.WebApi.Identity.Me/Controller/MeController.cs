using Bhbk.Lib.Identity.Infrastructure;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Controller
{
    [RoutePrefix("user")]
    public class MeController : BaseController
    {
        public MeController() { }

        public MeController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/{userID}/change-password"), HttpPut]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> ChangePassword(Guid userID, UserModel.Binding.ChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidNewPasswordConfirm);

            else if (!await UoW.CustomUserManager.CheckPasswordAsync(foundUser, model.CurrentPassword))
                return BadRequest(BaseLib.Statics.MsgUserInvalidCurrentPassword);

            else
            {
                IdentityResult result = await UoW.CustomUserManager.ChangePasswordAsync(foundUser.Id, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{userID}"), HttpPut]
        //[Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateMe(Guid userID, UserModel.Binding.Update model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (userID != model.Id)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(model.Id);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                foundUser.FirstName = model.FirstName;
                foundUser.LastName = model.LastName;

                IdentityResult result = await UoW.CustomUserManager.UpdateAsync(foundUser);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok(ModelFactory.Create(foundUser));
            }
        }
    }
}
