﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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
        public IActionResult AddToClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (!UoW.Users.AddToClaim(user, claim))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-login/{loginID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult AddToLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = UoW.Logins.Get(x => x.Id == loginID).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            if (!UoW.Users.AddToLogin(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-role/{roleID:guid}"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult AddToRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = UoW.Roles.Get(x => x.Id == roleID).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            if (!UoW.Users.AddToRole(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult CreateUserV1([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.Email == model.Email).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var issuer = UoW.Issuers.Get(x => x.Id == model.IssuerId).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                return NotFound(ModelState);
            }

            //ignore how bit may be set in model...
            model.HumanBeing = true;

            var result = UoW.Users.Create(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            if (UoW.InstanceType == InstanceContext.DeployedOrLocal)
            {
                var expire = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingTotpExpire).Single();

                var code = HttpUtility.UrlEncode(new PasswordlessTokenFactory(UoW.InstanceType.ToString())
                    .Generate(result.Email, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), result));

                var url = UrlFactory.GenerateConfirmEmailV1(Conf, result, code);
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
        public IActionResult CreateUserV1NoConfirm([FromBody] UserCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.Email == model.Email).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            //ignore how bit may be set in model...
            model.HumanBeing = false;

            var result = UoW.Users.Create(Mapper.Map<tbl_Users>(model));

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return Ok(Mapper.Map<UserModel>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteUserV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

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

            UoW.Users.Delete(user);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteUserRefreshesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == userID).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteUserRefreshV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == userID && x.Id == refreshID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{userID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(expr);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/msg-of-the-day/page"), HttpPost]
        public IActionResult GetMOTDsV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<MOTDType1Model>
                {
                    Data = Mapper.Map<IEnumerable<MOTDType1Model>>(
                        UoW.MOTDs.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_MotDType1>, IQueryable<tbl_MotDType1>>>>(
                                model.ToExpression<tbl_MotDType1>()))),

                    Total = UoW.MOTDs.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_MotDType1>, IQueryable<tbl_MotDType1>>>>(
                            model.ToPredicateExpression<tbl_MotDType1>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{userValue}"), HttpGet]
        public IActionResult GetUserV1([FromRoute] string userValue)
        {
            Guid userID;
            tbl_Users user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.Email == userValue).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserModel>(user));
        }

        [Route("v1/{userID:guid}/claims"), HttpGet]
        public IActionResult GetUserClaimsV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var claims = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<ClaimModel>>(claims));
        }

        [Route("v1/{userID:guid}/clients"), HttpGet]
        public IActionResult GetUserClientsV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var clients = UoW.Clients.Get(new QueryExpression<tbl_Clients>()
                .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == userID))).ToLambda());

            return Ok(Mapper.Map<IEnumerable<ClientModel>>(clients));
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public IActionResult GetUserLoginsV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var logins = UoW.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<LoginModel>>(logins));
        }

        [Route("v1/{userID:guid}/refreshes"), HttpGet]
        public IActionResult GetUserRefreshesV1([FromRoute] Guid userID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == userID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(expr);

            return Ok(Mapper.Map<IEnumerable<RefreshModel>>(refreshes));
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public IActionResult GetUserRolesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var roles = UoW.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleModel>>(roles));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetUsersV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<UserModel>
                {
                    Data = Mapper.Map<IEnumerable<UserModel>>(
                        UoW.Users.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Users>, IQueryable<tbl_Users>>>>(
                                model.ToExpression<tbl_Users>()))),

                    Total = UoW.Users.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Users>, IQueryable<tbl_Users>>>>(
                            model.ToPredicateExpression<tbl_Users>()))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());

                return BadRequest(ModelState);
            }
        }

        [Route("v1/{userID:guid}/remove-from-claim/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult RemoveFromClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }
            else if (!UoW.Users.RemoveFromClaim(user, claim))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-login/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult RemoveFromLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = UoW.Logins.Get(x => x.Id == loginID).SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }
            else if (!UoW.Users.RemoveFromLogin(user, login))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-role/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult RemoveFromRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = UoW.Roles.Get(x => x.Id == roleID).SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }
            else if (!UoW.Users.RemoveFromRole(user, role))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetUserGUID();

            if (!UoW.Users.IsPasswordSet(user.Id))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            if (!UoW.Users.RemovePassword(user))
                return StatusCode(StatusCodes.Status500InternalServerError);

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult SetPasswordV1([FromRoute] Guid userID, [FromBody] UserAddPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

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

            UoW.Users.SetPassword(user, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult UpdateUserV1([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == model.Id).SingleOrDefault();

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

            var result = UoW.Users.Update(Mapper.Map<tbl_Users>(model));

            UoW.Commit();

            return Ok(Mapper.Map<UserModel>(result));
        }
    }
}
