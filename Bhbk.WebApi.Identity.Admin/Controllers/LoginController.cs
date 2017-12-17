using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        public LoginController(IIdentityContext ioc)
            : base(ioc) { }

        [Route("v1/{loginID}/add/{userID}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddLoginToUser(Guid loginID, Guid userID, UserLoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var result = await IoC.UserMgmt.AddLoginAsync(user, 
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateLogin(LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByNameAsync(model.LoginProvider);

            if (login != null)
                return BadRequest(BaseLib.Statics.MsgLoginAlreadyExists);

            var create = new LoginFactory<LoginCreate>(model);
            var result = await IoC.LoginMgmt.CreateAsync(create.Devolve());

            return Ok(create.Evolve());
        }

        [Route("v1/{loginID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteLogin(Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            else if (login.LoginProvider == BaseLib.Statics.ApiDefaultLogin)
                return BadRequest(BaseLib.Statics.MsgLoginImmutable);

            if (!await IoC.LoginMgmt.DeleteAsync(loginID))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{loginID}"), HttpGet]
        public async Task<IActionResult> GetLogin(Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var result = new LoginFactory<AppLogin>(login);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetLogins()
        {
            IList<LoginResult> result = new List<LoginResult>();
            var logins = await IoC.LoginMgmt.GetListAsync();

            foreach (AppLogin entry in logins)
                result.Add(new LoginFactory<AppLogin>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1/{loginID}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsers(Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            IList<UserResult> result = new List<UserResult>();
            var users = await IoC.LoginMgmt.GetUsersListAsync(loginID);

            foreach (AppUser entry in users)
                result.Add(new UserFactory<AppUser>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1/{loginID}/remove/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveLoginFromUser(Guid loginID, Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else
            {
                var result = await IoC.UserMgmt.RemoveFromProviderAsync(user, IoC.LoginMgmt.Store.FindById(loginID).LoginProvider, string.Empty);

                if (!result.Succeeded)
                    return GetErrorResult(result);

                else
                    return NoContent();
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateLogin(LoginUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByIdAsync(model.Id);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            else
            {
                var update = new LoginFactory<LoginUpdate>(model);
                var result = await IoC.LoginMgmt.UpdateAsync(update.Devolve());

                return Ok(update.Evolve());
            }
        }
    }
}