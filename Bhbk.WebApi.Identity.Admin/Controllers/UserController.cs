using Bhbk.Lib.Alert.Factory;
using Bhbk.Lib.Alert.Interop;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1/{userID:guid}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var result = await UoW.CustomUserMgr.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateUserV1([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();
            
            var exists = await UoW.CustomUserMgr.FindByEmailAsync(model.Email);

            //if (exists != null)
            //    return BadRequest(new { error = Statics.MsgUserAlreadyExists });

            if (exists != null)
                return BadRequest(Strings.MsgUserAlreadyExists);

            var client = await UoW.ClientRepo.GetAsync(model.ClientId);

            if (client == null)
                return NotFound(Strings.MsgClientNotExist);

            var create = new UserFactory<UserCreate>(model);

            //ignore how bit may be set in model...
            create.HumanBeing = true;

            var result = await UoW.CustomUserMgr.CreateAsync(create.ToStore());

            if (!result.Succeeded)
                return GetErrorResult(result);

            var user = await UoW.CustomUserMgr.FindByIdAsync(create.Id.ToString());

            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (UoW.Situation == ContextType.Live)
            {
                var alert = new AlertClient(Conf, UoW.Situation);

                if (alert == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var code = HttpUtility.UrlEncode(await new ProtectProvider(UoW.Situation.ToString())
                    .GenerateAsync(user.Email, TimeSpan.FromSeconds(UoW.ConfigRepo.DefaultsAuthorizationCodeExpire), user));

                var url = LinkBuilder.ConfirmEmail(Conf, user, code);

                var email = await alert.EnqueueEmailV1(Jwt.AccessToken,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = user.Email,
                        FromDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                        ToId = user.Id,
                        ToEmail = user.Email,
                        ToDisplay = string.Format("{0} {1}", user.FirstName, user.LastName),
                        Subject = string.Format("{0} {1}", client.Name, Strings.ApiMsgConfirmNewUserSubject),
                        HtmlContent = Strings.ApiTemplateConfirmNewUser(client, user, url)
                    });

                if (!email.IsSuccessStatusCode)
                    return BadRequest(Strings.MsgSysQueueEmailError);
            }

            return Ok((new UserFactory<AppUser>(user)).ToClient());
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var exists = await UoW.CustomUserMgr.FindByEmailAsync(model.Email);

            if (exists != null)
                return BadRequest(Strings.MsgUserAlreadyExists);

            var create = new UserFactory<UserCreate>(model);

            //ignore how bit may be set in model...
            create.HumanBeing = false;

            var result = await UoW.CustomUserMgr.CreateAsync(create.ToStore());

            if (!result.Succeeded)
                return GetErrorResult(result);

            var user = await UoW.CustomUserMgr.FindByIdAsync(create.Id.ToString());

            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok((new UserFactory<AppUser>(user)).ToClient());
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteUserV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(Strings.MsgUserImmutable);

            user.ActorId = GetUserGUID();

            var result = await UoW.CustomUserMgr.DeleteAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/{userID:guid}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.ToClient());
        }

        [Route("v1/{email}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] string email)
        {
            var user = await UoW.CustomUserMgr.FindByEmailAsync(email);

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.ToClient());
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var logins = await UoW.CustomUserMgr.GetLoginsAsync(user);

            var result = (await UoW.LoginRepo.GetAsync(x => logins.Contains(x.Id.ToString())))
                .Select(x => new LoginFactory<AppLogin>(x).ToClient());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/audiences"), HttpGet]
        public async Task<IActionResult> GetUserAudiencesV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var audiences = await UoW.CustomUserMgr.GetAudiencesAsync(user);

            var result = (await UoW.AudienceRepo.GetAsync(x => audiences.Contains(x.Id.ToString())))
                .Select(x => new AudienceFactory<AppAudience>(x).ToClient());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            var roles = await UoW.CustomUserMgr.GetRolesResultIdAsync(user);

            var result = UoW.CustomRoleMgr.Store.Get(x => roles.Contains(x.Id.ToString()))
                .Select(x => new RoleFactory<AppRole>(x).ToClient());

            return Ok(result);
        }

        [Route("v1"), HttpGet]
        public IActionResult GetUsersV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var users = UoW.CustomUserMgr.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(model.Skip)
                .Take(model.Take);

            var result = users.Select(x => new UserFactory<AppUser>(x).ToClient());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/remove-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (!await UoW.CustomUserMgr.HasPasswordAsync(user))
                return BadRequest(Strings.MsgUserInvalidPassword);

            user.ActorId = GetUserGUID();

            var result = await UoW.CustomUserMgr.RemovePasswordAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> SetPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.CustomUserMgr.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(Strings.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var remove = await UoW.CustomUserMgr.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await UoW.CustomUserMgr.AddPasswordAsync(user, model.NewPassword);

            if (!add.Succeeded)
                return GetErrorResult(add);

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateUserV1([FromBody] UserUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var user = await UoW.CustomUserMgr.FindByIdAsync(model.Id.ToString());

            if (user == null)
                return NotFound(Strings.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(Strings.MsgUserImmutable);

            var update = new UserFactory<UserUpdate>(model);
            var result = await UoW.CustomUserMgr.UpdateAsync(update.ToStore());

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(update.ToClient());
        }
    }
}
