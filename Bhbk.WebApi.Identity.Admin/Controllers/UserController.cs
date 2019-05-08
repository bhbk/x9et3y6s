using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        private UserProvider _provider;

        public UserController(IConfiguration conf, IContextService instance)
        {
            _provider = new UserProvider(conf, instance);
        }

        [Route("v1/{userID:guid}/add-to-claim/{claimID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> AddToClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == claimID)).SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddToClaimAsync(user, claim))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-login/{loginID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> AddToLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddToLoginAsync(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-role/{roleID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> AddToRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddToRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateUserV1([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == model.IssuerId)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                return NotFound(ModelState);
            }

            //ignore how bit may be set in model...
            model.HumanBeing = true;

            var result = await UoW.UserRepo.CreateAsync(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            if (UoW.InstanceType == InstanceContext.DeployedOrLocal)
            {
                var expire = (await UoW.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingTotpExpire)).Single();

                var code = HttpUtility.UrlEncode(await new ProtectHelper(UoW.InstanceType.ToString())
                    .GenerateAsync(result.Email, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), result));

                var url = UrlHelper.GenerateConfirmEmailV1(Conf, result, code);
                var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

                alert.Email_EnqueueV1(new EmailCreate()
                {
                    FromId = result.Id,
                    FromEmail = result.Email,
                    FromDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                    ToId = result.Id,
                    ToEmail = result.Email,
                    ToDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                    Subject = string.Format("{0} {1}", issuer.Name, Constants.MsgConfirmNewUserSubject),
                    HtmlContent = Constants.TemplateConfirmNewUser(issuer, result, url)
                });
            }

            return Ok(Mapper.Map<UserModel>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            //ignore how bit may be set in model...
            model.HumanBeing = false;

            var result = await UoW.UserRepo.CreateAsync(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(Mapper.Map<UserModel>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteUserV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (user.Immutable)
            {
                ModelState.AddModelError(MessageType.UserImmutable.ToString(), $"User:{userID}");
                return BadRequest(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.DeleteAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteUserRefreshesV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            foreach (var refreshes in await UoW.RefreshRepo.GetAsync(x => x.UserId == userID))
            {
                if (!await UoW.RefreshRepo.DeleteAsync(refreshes.Id))
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> DeleteUserRefreshV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var refresh = (await UoW.RefreshRepo.GetAsync(x => x.UserId == userID
                && x.Id == refreshID)).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{userID}");
                return NotFound(ModelState);
            }

            if (!await UoW.RefreshRepo.DeleteAsync(refresh.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/msg-of-the-day/page"), HttpPost]
        public async Task<IActionResult> GetMOTDsV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_MotDType1, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Author.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.Quote.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.UserRepo.CountMOTDAsync(preds);
                var result = await UoW.UserRepo.GetMOTDAsync(preds,
                    null,
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<MotDType1Model>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{userValue}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] string userValue)
        {
            Guid userID;
            tbl_Users user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == userValue)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserModel>(user));
        }

        [Route("v1/{userID:guid}/claims"), HttpGet]
        public async Task<IActionResult> GetUserClaimsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var claims = await UoW.UserRepo.GetClaimsAsync(user.Id);

            var result = (await UoW.ClaimRepo.GetAsync(x => claims.Contains(x)))
                .Select(x => Mapper.Map<ClaimModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetUserClientsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var clients = await UoW.UserRepo.GetClientsAsync(user.Id);

            var result = (await UoW.ClientRepo.GetAsync(x => clients.Contains(x)))
                .Select(x => Mapper.Map<ClientModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var logins = await UoW.UserRepo.GetLoginsAsync(user.Id);

            var result = (await UoW.LoginRepo.GetAsync(x => logins.Contains(x)))
                .Select(x => Mapper.Map<LoginModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/refreshes"), HttpGet]
        public async Task<IActionResult> GetUserRefreshesV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var refreshes = await UoW.RefreshRepo.GetAsync(x => x.UserId == userID);

            var result = refreshes.Select(x => Mapper.Map<RefreshModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var roles = await UoW.UserRepo.GetRolesAsync(user.Id);

            var result = (await UoW.RoleRepo.GetAsync(x => roles.Contains(x)))
                .Select(x => Mapper.Map<RoleModel>(x));

            return Ok(result);
        }

        [Route("v1/page"), HttpPost]
        public async Task<IActionResult> GetUsersV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<tbl_Users, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Email.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.PhoneNumber.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.FirstName.Contains(model.Filter, StringComparison.OrdinalIgnoreCase)
                || x.LastName.Contains(model.Filter, StringComparison.OrdinalIgnoreCase);

            try
            {
                var total = await UoW.UserRepo.CountAsync(preds);
                var result = await UoW.UserRepo.GetAsync(preds,
                    x => x.Include(r => r.tbl_UserRoles).ThenInclude(r => r.Role),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = Mapper.Map<IEnumerable<UserModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{userID:guid}/remove-from-claim/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> RemoveFromClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Id == claimID)).SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }
            else if (!await UoW.UserRepo.RemoveFromClaimAsync(user, claim))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-login/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> RemoveFromLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }
            else if (!await UoW.UserRepo.RemoveFromLoginAsync(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-role/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> RemoveFromRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = (await UoW.RoleRepo.GetAsync(x => x.Id == roleID)).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }
            else if (!await UoW.UserRepo.RemoveFromRoleAsync(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.IsPasswordSetAsync(user.Id))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!await UoW.UserRepo.RemovePasswordAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> SetPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!await UoW.UserRepo.SetPasswordAsync(user.Id, model.NewPassword))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public async Task<IActionResult> UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.Id}");
                return NotFound(ModelState);
            }
            else if (user.Immutable
                && user.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.UserImmutable.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.UserRepo.UpdateAsync(Mapper.Map<tbl_Users>(model));

            await UoW.CommitAsync();

            return Ok(Mapper.Map<UserModel>(result));
        }
    }
}
