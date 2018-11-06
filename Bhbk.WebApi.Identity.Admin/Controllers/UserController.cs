using Bhbk.Lib.Alert.Factory;
using Bhbk.Lib.Alert.Helpers;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.EntityModels;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        [Route("v1/{userID:guid}/add-password"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> AddPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var result = await UoW.UserRepo.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateUserV1([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var exists = (await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).SingleOrDefault();

            if (exists != null)
                return BadRequest(Strings.MsgUserAlreadyExists);

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == model.IssuerId)).SingleOrDefault();

            if (issuer == null)
                return NotFound(Strings.MsgIssuerNotExist);

            //ignore how bit may be set in model...
            model.HumanBeing = true;

            var result = await UoW.UserRepo.CreateAsync(UoW.Convert.Map<AppUser>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            if (UoW.Situation == ContextType.Live)
            {
                var alert = new AlertClient(Conf, UoW.Situation);

                if (alert == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var code = HttpUtility.UrlEncode(await new ProtectProvider(UoW.Situation.ToString())
                    .GenerateAsync(result.Email, TimeSpan.FromSeconds(UoW.ConfigRepo.DefaultsAuthorizationCodeExpire), result));

                var url = UrlBuilder.ConfirmEmail(Conf, result, code);

                var email = await alert.EnqueueEmailV1(Jwt.AccessToken,
                    new EmailCreate()
                    {
                        FromId = result.Id,
                        FromEmail = result.Email,
                        FromDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                        ToId = result.Id,
                        ToEmail = result.Email,
                        ToDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                        Subject = string.Format("{0} {1}", issuer.Name, Strings.ApiMsgConfirmNewUserSubject),
                        HtmlContent = Strings.ApiTemplateConfirmNewUser(issuer, result, url)
                    });

                if (!email.IsSuccessStatusCode)
                    return BadRequest(Strings.MsgSysQueueEmailError);
            }

            return Ok(UoW.Convert.Map<UserModel>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var exists = (await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).SingleOrDefault();

            if (exists != null)
                return BadRequest(Strings.MsgUserAlreadyExists);

            //ignore how bit may be set in model...
            model.HumanBeing = false;

            var result = await UoW.UserRepo.CreateAsync(UoW.Convert.Map<AppUser>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<UserModel>(result));
        }

        [Route("v1/{userID:guid}/claim"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateUserClaimV1([FromRoute] Guid userID, [FromBody] UserClaimCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            //var result = await UoW.UserMgr.AddClaimAsync(user, model);

            //if (!result.Succeeded)
            //    return GetErrorResult(result);

            return Ok(model);
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteUserV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(Strings.MsgUserImmutable);

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.DeleteAsync(user))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteUserClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            //var result = await UoW.UserMgr.RemoveClaimAsync(user, claim);

            //if (!result.Succeeded)
            //    return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/{userValue}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] string userValue)
        {
            Guid userID;
            AppUser user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == userValue)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            return Ok(UoW.Convert.Map<UserModel>(user));
        }

        [Route("v1/{userID:guid}/claims"), HttpGet]
        public async Task<IActionResult> GetUserClaimsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserInvalid);

            return Ok(await UoW.UserRepo.GetClaimsAsync(user));
        }

        [Route("v1/{userID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetUserClientsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var clients = await UoW.UserRepo.GetClientsAsync(user);

            var result = (await UoW.ClientRepo.GetAsync(x => clients.Contains(x.Id.ToString())))
                .Select(x => UoW.Convert.Map<ClientModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var logins = await UoW.UserRepo.GetLoginsAsync(user);

            var result = (await UoW.LoginRepo.GetAsync(x => logins.Contains(x.Id.ToString())))
                .Select(x => UoW.Convert.Map<LoginModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var roles = await UoW.UserRepo.GetRolesAsync_Deprecate(user);

            var result = (await UoW.RoleRepo.GetAsync(x => roles.Contains(x.Id.ToString())))
                .Select(x => UoW.Convert.Map<RoleModel>(x));

            return Ok(result);
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetUsersV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Expression<Func<AppUser, bool>> expr;

            if (string.IsNullOrEmpty(model.Filter))
                expr = x => true;
            else
                expr = x => x.Email.ToLower().Contains(model.Filter.ToLower())
                || x.PhoneNumber.ToLower().Contains(model.Filter.ToLower())
                || x.FirstName.ToLower().Contains(model.Filter.ToLower())
                || x.LastName.ToLower().Contains(model.Filter.ToLower());

            var total = await UoW.UserRepo.Count(expr);
            var list = await UoW.UserRepo.GetAsync(expr,
                x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)).Skip(model.Skip).Take(model.Take),
                x => x.Include(y => y.AppUserLogin)
                    .Include(y => y.AppUserRole));

            var result = list.Select(x => UoW.Convert.Map<UserModel>(x));

            return Ok(new { Count = total, List = result });
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!await UoW.UserRepo.IsPasswordSetAsync(user))
                return BadRequest(Strings.MsgUserInvalidPassword);

            user.ActorId = GetUserGUID();

            var result = await UoW.UserRepo.RemovePasswordAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> SetPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var remove = await UoW.UserRepo.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await UoW.UserRepo.AddPasswordAsync(user, model.NewPassword);

            if (!add.Succeeded)
                return GetErrorResult(add);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateUserV1([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(Strings.MsgUserImmutable);

            var result = await UoW.UserRepo.UpdateAsync(UoW.Convert.Map<AppUser>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Convert.Map<UserModel>(result));
        }
    }
}
