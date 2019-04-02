﻿using Bhbk.Lib.Core.DomainModels;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.DomainModels.Alert;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Providers;
using Bhbk.Lib.Identity.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using System.Net.Http;
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
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!await UoW.UserRepo.AddPasswordAsync(user.Id, model.NewPassword))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-login/{loginID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> AddToLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MsgType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            if (!await UoW.UserRepo.AddToLoginAsync(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateUserV1([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).Any())
            {
                ModelState.AddModelError(MsgType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Id == model.IssuerId)).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MsgType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                return NotFound(ModelState);
            }

            //ignore how bit may be set in model...
            model.HumanBeing = true;

            var result = await UoW.UserRepo.CreateAsync(model);

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            if (UoW.Situation == ExecutionType.Live)
            {
                var alert = new AlertClient(Conf, UoW.Situation, new HttpClient());

                if (alert == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var code = HttpUtility.UrlEncode(await new ProtectProvider(UoW.Situation.ToString())
                    .GenerateAsync(result.Email, TimeSpan.FromSeconds(UoW.ConfigRepo.DefaultsExpireAuthCodeTOTP), result));

                var url = UrlBuilder.GenerateConfirmEmail(Conf, result, code);

                var email = await alert.Enqueue_EmailV1(Jwt.AccessToken.ToString(),
                    new EmailCreate()
                    {
                        FromId = result.Id,
                        FromEmail = result.Email,
                        FromDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                        ToId = result.Id,
                        ToEmail = result.Email,
                        ToDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                        Subject = string.Format("{0} {1}", issuer.Name, Strings.MsgConfirmNewUserSubject),
                        HtmlContent = Strings.TemplateConfirmNewUser(issuer, result, url)
                    });

                if (!email.IsSuccessStatusCode)
                    return BadRequest(await email.Content.ReadAsStringAsync());
            }

            return Ok(UoW.Transform.Map<UserModel>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((await UoW.UserRepo.GetAsync(x => x.Email == model.Email)).Any())
            {
                ModelState.AddModelError(MsgType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            //ignore how bit may be set in model...
            model.HumanBeing = false;

            var result = await UoW.UserRepo.CreateAsync(model);

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<UserModel>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> DeleteUserV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (user.Immutable)
            {
                ModelState.AddModelError(MsgType.UserImmutable.ToString(), $"User:{userID}");
                return BadRequest(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.DeleteAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userValue}"), HttpGet]
        public async Task<IActionResult> GetUserV1([FromRoute] string userValue)
        {
            Guid userID;
            TUsers user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();
            else
                user = (await UoW.UserRepo.GetAsync(x => x.Email == userValue)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(UoW.Transform.Map<UserModel>(user));
        }

        [Route("v1/{userID:guid}/claims"), HttpGet]
        public async Task<IActionResult> GetUserClaimsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            return Ok(await UoW.UserRepo.GetClaimsAsync(user.Id));
        }

        [Route("v1/{userID:guid}/clients"), HttpGet]
        public async Task<IActionResult> GetUserClientsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var clients = await UoW.UserRepo.GetClientsAsync(user.Id);

            var result = (await UoW.ClientRepo.GetAsync(x => clients.Contains(x)))
                .Select(x => UoW.Transform.Map<ClientModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public async Task<IActionResult> GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var logins = await UoW.UserRepo.GetLoginsAsync(user.Id);

            var result = (await UoW.LoginRepo.GetAsync(x => logins.Contains(x)))
                .Select(x => UoW.Transform.Map<LoginModel>(x));

            return Ok(result);
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var roles = await UoW.UserRepo.GetRolesAsync(user.Id);

            var result = (await UoW.RoleRepo.GetAsync(x => roles.Contains(x)))
                .Select(x => UoW.Transform.Map<RoleModel>(x));

            return Ok(result);
        }

        [Route("v1/pages"), HttpPost]
        public async Task<IActionResult> GetUsersPageV1([FromBody] CascadePager model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*
             * tidbits below need enhancment, just tinkering...
             */

            Expression<Func<TUsers, bool>> preds;

            if (string.IsNullOrEmpty(model.Filter))
                preds = x => true;
            else
                preds = x => x.Email.ToLower().Contains(model.Filter.ToLower())
                || x.PhoneNumber.ToLower().Contains(model.Filter.ToLower())
                || x.FirstName.ToLower().Contains(model.Filter.ToLower())
                || x.LastName.ToLower().Contains(model.Filter.ToLower());

            try
            {
                var total = await UoW.UserRepo.CountAsync(preds);
                var result = await UoW.UserRepo.GetAsync(preds,
                    x => x.Include(r => r.TUserRoles),
                    x => x.OrderBy(string.Format("{0} {1}", model.Orders.First().Item1, model.Orders.First().Item2)),
                    model.Skip,
                    model.Take);

                return Ok(new { Count = total, List = UoW.Transform.Map<IEnumerable<UserModel>>(result) });
            }
            catch (ParseException ex)
            {
                ModelState.AddModelError(MsgType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{userID:guid}/remove-from-login/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemoveLoginFromUserV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = (await UoW.LoginRepo.GetAsync(x => x.Id == loginID)).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MsgType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }
            else if (!await UoW.UserRepo.RemoveFromLoginAsync(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == userID)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (!await UoW.UserRepo.IsPasswordSetAsync(user.Id))
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!await UoW.UserRepo.RemovePasswordAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

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
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                ModelState.AddModelError(MsgType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!await UoW.UserRepo.RemovePasswordAsync(user.Id))
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!await UoW.UserRepo.AddPasswordAsync(user.Id, model.NewPassword))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = (await UoW.UserRepo.GetAsync(x => x.Id == model.Id)).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"User:{model.Id}");
                return NotFound(ModelState);
            }

            if (user.Immutable)
            {
                ModelState.AddModelError(MsgType.UserImmutable.ToString(), $"User:{model.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = await UoW.UserRepo.UpdateAsync(UoW.Transform.Map<TUsers>(model));

            await UoW.CommitAsync();

            return Ok(UoW.Transform.Map<UserModel>(result));
        }
    }
}
