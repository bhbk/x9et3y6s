using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        public LoginController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/{loginID}/add/{userID}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddLoginToUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID, [FromBody] UserLoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = await IoC.UserMgmt.AddLoginAsync(user,
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await IoC.LoginMgmt.FindByNameAsync(model.LoginProvider);

            if (check != null)
                return BadRequest(BaseLib.Statics.MsgLoginAlreadyExists);

            var login = new LoginFactory<LoginCreate>(model);

            var result = await IoC.LoginMgmt.CreateAsync(login.Devolve());

            return Ok(login.Evolve());
        }

        [Route("v1/{loginID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            else if (login.Immutable)
                return BadRequest(BaseLib.Statics.MsgLoginImmutable);

            login.ActorId = GetUserGUID();

            if (!await IoC.LoginMgmt.DeleteAsync(login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{loginID:guid}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var result = new LoginFactory<AppLogin>(login);

            return Ok(result.Evolve());
        }

        [Route("v1/{loginName}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] string loginName)
        {
            var login = await IoC.LoginMgmt.FindByNameAsync(loginName);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var result = new LoginFactory<AppLogin>(login);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public IActionResult GetLoginsV1([FromQuery] PagingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var logins = IoC.LoginMgmt.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(Convert.ToInt32((model.PageNumber - 1) * model.PageSize))
                .Take(Convert.ToInt32(model.PageSize));

            var result = logins.Select(x => new LoginFactory<AppLogin>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{loginID}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var users = await IoC.LoginMgmt.GetUsersListAsync(loginID);

            var result = users.Select(x => new UserFactory<AppUser>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{loginID}/remove/{userID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveLoginFromUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await IoC.LoginMgmt.FindByIdAsync(loginID);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var result = await IoC.UserMgmt.RemoveLoginAsync(user, IoC.LoginMgmt.Store.FindById(loginID).LoginProvider, string.Empty);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateLoginV1([FromBody] LoginUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var login = await IoC.LoginMgmt.FindByIdAsync(model.Id);

            if (login == null)
                return BadRequest(BaseLib.Statics.MsgLoginInvalid);

            var update = new LoginFactory<AppLogin>(login);
            update.Update(model);

            var result = await IoC.LoginMgmt.UpdateAsync(update.Devolve());

            return Ok(update.Evolve());
        }
    }
}