using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
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

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        public LoginController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1/{loginID:guid}/add/{userID:guid}"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddLoginToUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID, [FromBody] UserLoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await UoW.LoginRepo.GetAsync(loginID);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.AddLoginAsync(user,
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.LoginRepo.GetAsync(x => x.LoginProvider == model.LoginProvider);

            if (check.Any())
                return BadRequest(Strings.MsgLoginAlreadyExists);

            var result = await UoW.LoginRepo.CreateAsync(UoW.Convert.Map<AppLogin>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<LoginResult>(result));
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = await UoW.LoginRepo.GetAsync(loginID);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            else if (login.Immutable)
                return BadRequest(Strings.MsgLoginImmutable);

            login.ActorId = GetUserGUID();

            if (!await UoW.LoginRepo.DeleteAsync(login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{loginID:guid}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] Guid loginID)
        {
            var login = await UoW.LoginRepo.GetAsync(loginID);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            return Ok(UoW.Convert.Map<LoginResult>(login));
        }

        [Route("v1/{loginName}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] string loginName)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.LoginProvider == loginName)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            return Ok(UoW.Convert.Map<LoginResult>(login));
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetLoginsV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var logins = await UoW.LoginRepo.GetAsync(x => true,
                x => x.OrderBy(model.OrderBy).Skip(model.Skip).Take(model.Take));

            var result = logins.Select(x => UoW.Convert.Map<AppLogin>(x));

            return Ok(result);
        }

        [Route("v1/{loginID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = await UoW.LoginRepo.GetAsync(loginID);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var users = await UoW.LoginRepo.GetUsersAsync(loginID);

            var result = users.Select(x => UoW.Convert.Map<UserResult>(x));

            return Ok(result);
        }

        [Route("v1/{loginID:guid}/remove/{userID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemoveLoginFromUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = await UoW.LoginRepo.GetAsync(loginID);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.CustomUserMgr.RemoveLoginAsync(user, login.LoginProvider, string.Empty);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateLoginV1([FromBody] LoginUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var login = await UoW.LoginRepo.GetAsync(model.Id);

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var result = await UoW.LoginRepo.UpdateAsync(UoW.Convert.Map<AppLogin>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<LoginResult>(result));
        }
    }
}