using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        public LoginController(IIdentityContext context)
            : base(context) { }

        [Route("v1/{loginID}/add/{userID}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddLoginToUser(Guid loginID, Guid userID, UserLoginCreate userLogin)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var create = Context.LoginMgmt.Store.Mf.Create.DoIt(userLogin);
                var result = await Context.UserMgmt.AddLoginAsync(user.Id, create);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateLogin(LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await Context.LoginMgmt.FindByNameAsync(model.LoginProvider);

            if (login == null)
            {
                var create = Context.UserMgmt.Store.Mf.Create.DoIt(model);
                var result = await Context.LoginMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgLoginAlreadyExists);
        }

        [Route("v1/{loginID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteLogin(Guid loginID)
        {
            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            if (!await Context.LoginMgmt.DeleteAsync(loginID))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return Ok();
        }

        [Route("v1/{loginID}"), HttpGet]
        public async Task<IActionResult> GetLogin(Guid loginID)
        {
            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            else
                return Ok(login);
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetLogins()
        {
            return Ok(await Context.LoginMgmt.GetListAsync());
        }

        [Route("v1/{loginID}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsers(Guid loginID)
        {
            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            else
                return Ok(await Context.LoginMgmt.GetUsersListAsync(loginID));
        }

        [Route("v1/{loginID}/remove/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveLoginFromUser(Guid loginID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            var user = await Context.UserMgmt.FindByIdAsync(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IdentityResult result = await Context.UserMgmt.RemoveFromProviderAsync(userID, Context.LoginMgmt.Store.FindById(loginID).LoginProvider);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return Ok();
            }
        }

        [Route("v1/{loginID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateLogin(Guid loginID, LoginUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (loginID != model.Id)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var login = await Context.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginNotExist);

            else
            {
                var update = Context.LoginMgmt.Store.Mf.Update.DoIt(model);
                var result = await Context.LoginMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}