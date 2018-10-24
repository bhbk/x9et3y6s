using Bhbk.Lib.Alert.Factory;
using Bhbk.Lib.Alert.Interop;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        public UserController() { }

        public UserController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/{userID:guid}/add-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> AddPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var result = await IoC.UserMgmt.AddPasswordAsync(user, model.NewPassword);

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
            
            var exists = await IoC.UserMgmt.FindByEmailAsync(model.Email);

            //if (exists != null)
            //    return BadRequest(new { error = BaseLib.Statics.MsgUserAlreadyExists });

            if (exists != null)
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);

            var client = await IoC.ClientMgmt.FindByIdAsync(model.ClientId);

            if (client == null)
                return NotFound(BaseLib.Statics.MsgClientNotExist);

            var create = new UserFactory<UserCreate>(model);

            //ignore how bit may be set in model...
            create.HumanBeing = true;

            var result = await IoC.UserMgmt.CreateAsync(create.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            var user = await IoC.UserMgmt.FindByIdAsync(create.Id.ToString());

            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (IoC.Status == ContextType.Live)
            {
                var alert = new AlertClient(Conf, IoC.Status);

                if (alert == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var code = HttpUtility.UrlEncode(await new ProtectProvider(IoC.Status.ToString())
                    .GenerateAsync(user.Email, TimeSpan.FromSeconds(IoC.ConfigMgmt.Store.DefaultsAuthorizationCodeExpire), user));

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
                        Subject = string.Format("{0} {1}", client.Name, BaseLib.Statics.ApiMsgConfirmNewUserSubject),
                        HtmlContent = BaseLib.Statics.ApiTemplateConfirmNewUser(client, user, url)
                    });

                if (!email.IsSuccessStatusCode)
                    return BadRequest(BaseLib.Statics.MsgSysQueueEmailError);
            }

            return Ok((new UserFactory<AppUser>(user)).Evolve());
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var exists = await IoC.UserMgmt.FindByEmailAsync(model.Email);

            if (exists != null)
                return BadRequest(BaseLib.Statics.MsgUserAlreadyExists);

            var create = new UserFactory<UserCreate>(model);

            //ignore how bit may be set in model...
            create.HumanBeing = false;

            var result = await IoC.UserMgmt.CreateAsync(create.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            var user = await IoC.UserMgmt.FindByIdAsync(create.Id.ToString());

            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok((new UserFactory<AppUser>(user)).Evolve());
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteUserV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            user.ActorId = GetUserGUID();

            var result = await IoC.UserMgmt.DeleteAsync(user);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return NoContent();
        }

        [Route("v1/{userID:guid}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{email}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] string email)
        {
            var user = await IoC.UserMgmt.FindByEmailAsync(email);

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var result = new UserFactory<AppUser>(user);

            return Ok(result.Evolve());
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var logins = await IoC.UserMgmt.GetLoginsAsync(user);

            var result = IoC.LoginMgmt.Store.Get(x => logins.Contains(x.Id.ToString()))
                .Select(x => new LoginFactory<AppLogin>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/audiences"), HttpGet]
        public async Task<IActionResult> GetUserAudiencesV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var audiences = await IoC.UserMgmt.GetAudiencesAsync(user);

            var result = IoC.AudienceMgmt.Store.Get(x => audiences.Contains(x.Id.ToString()))
                .Select(x => new AudienceFactory<AppAudience>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            var roles = await IoC.UserMgmt.GetRolesResultIdAsync(user);

            var result = IoC.RoleMgmt.Store.Get(x => roles.Contains(x.Id.ToString()))
                .Select(x => new RoleFactory<AppRole>(x).Evolve());

            return Ok(result);
        }

        [Route("v1"), HttpGet]
        public IActionResult GetUsersV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var users = IoC.UserMgmt.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(model.Skip)
                .Take(model.Take);

            var result = users.Select(x => new UserFactory<AppUser>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{userID:guid}/remove-password"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (!await IoC.UserMgmt.HasPasswordAsync(user))
                return BadRequest(BaseLib.Statics.MsgUserInvalidPassword);

            user.ActorId = GetUserGUID();

            var result = await IoC.UserMgmt.RemovePasswordAsync(user);

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

            var user = await IoC.UserMgmt.FindByIdAsync(userID.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (model.NewPassword != model.NewPasswordConfirm)
                return BadRequest(BaseLib.Statics.MsgUserInvalidPasswordConfirm);

            user.ActorId = GetUserGUID();

            var remove = await IoC.UserMgmt.RemovePasswordAsync(user);

            if (!remove.Succeeded)
                return GetErrorResult(remove);

            var add = await IoC.UserMgmt.AddPasswordAsync(user, model.NewPassword);

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

            var user = await IoC.UserMgmt.FindByIdAsync(model.Id.ToString());

            if (user == null)
                return NotFound(BaseLib.Statics.MsgUserNotExist);

            else if (user.Immutable)
                return BadRequest(BaseLib.Statics.MsgUserImmutable);

            var update = new UserFactory<AppUser>(user);
            update.Update(model);

            var result = await IoC.UserMgmt.UpdateAsync(update.Devolve());

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok(update.Evolve());
        }
    }
}
