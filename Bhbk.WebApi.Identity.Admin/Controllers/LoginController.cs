using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        public LoginController() { }

        [Route("v1/{loginID:guid}/add/{userID:guid}"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> AddLoginToUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID, [FromBody] UserLoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserRepo.AddLoginAsync(user.Id,
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateLoginV1([FromBody] LoginCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.LoginRepo.GetAsync(x => x.LoginProvider == model.LoginProvider);

            if (check.Any())
                return BadRequest(Strings.MsgLoginAlreadyExists);

            var result = await UoW.LoginRepo.CreateAsync(model);

            await UoW.CommitAsync();

            return Ok(result);
        }

        [Route("v1/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteLoginV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            else if (login.Immutable)
                return BadRequest(Strings.MsgLoginImmutable);

            login.ActorId = GetUserGUID();

            if (!await UoW.LoginRepo.DeleteAsync(login.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{loginValue}"), HttpGet]
        public async Task<IActionResult> GetLoginV1([FromRoute] string loginValue)
        {
            Guid loginID;
            LoginModel login;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(loginValue, out loginID))
                login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();
            else
                login = (await UoW.LoginRepo.GetAsync(x => x.LoginProvider == loginValue)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            return Ok(login);
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetLoginsPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<LoginModel, bool>> preds;
            Expression<Func<LoginModel, object>> ords = x => string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2);

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.LoginProvider.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.LoginRepo.Count(preds);
            var result = await UoW.LoginRepo.GetAsync(preds, ords, null, model.Skip, model.Take);

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{loginID:guid}/users"), HttpGet]
        public async Task<IActionResult> GetLoginUsersV1([FromRoute] Guid loginID)
        {
            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var users = await UoW.LoginRepo.GetUsersAsync(loginID);

            var result = users.Select(x => UoW.Convert.Map<UserModel>(x));

            return Ok(result);
        }

        [Route("v1/{loginID:guid}/remove/{userID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemoveLoginFromUserV1([FromRoute] Guid loginID, [FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = await UoW.UserRepo.RemoveLoginAsync(user.Id, login.LoginProvider, string.Empty);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateLoginV1([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (login == null)
                return NotFound(Strings.MsgLoginNotExist);

            var result = await UoW.LoginRepo.UpdateAsync(model);

            await UoW.CommitAsync();

            return Ok(result);
        }
    }
}