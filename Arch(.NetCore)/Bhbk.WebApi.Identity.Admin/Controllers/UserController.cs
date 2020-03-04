using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Primitives;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    [Authorize(Policy = Constants.PolicyForUsers)]
    public class UserController : BaseController
    {
        private UserProvider _provider;

        public UserController(IConfiguration conf, IContextService instance)
        {
            _provider = new UserProvider(conf, instance);
        }

        [Route("v1/{userID:guid}/add-to-claim/{claimID:guid}"), HttpGet]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult AddToClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult AddToLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = UoW.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult AddToRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = UoW.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult CreateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.Email == model.Email).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            var issuer = UoW.Issuers.Get(x => x.Id == model.IssuerId)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                return NotFound(ModelState);
            }

            //ignore how these may be set in model...
            model.HumanBeing = true;
            model.TwoFactorEnabled = false;
            model.EmailConfirmed = false;
            model.PasswordConfirmed = false;
            model.PhoneNumberConfirmed = false;

            if (!new ValidationHelper().ValidateEmail(model.Email).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            var result = UoW.Users.Create(Mapper.Map<tbl_Users>(model));

            UoW.Commit();

            if (UoW.InstanceType == InstanceContext.DeployedOrLocal)
            {
                var expire = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingTotpExpire).Single();

                var code = HttpUtility.UrlEncode(new PasswordlessTokenFactory(UoW.InstanceType.ToString())
                    .Generate(result.Email, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), result));

                var url = UrlFactory.GenerateConfirmEmailV1(Conf, result, code);
                var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

                alert.Email_EnqueueV1(new EmailV1()
                {
                    FromId = result.Id,
                    FromEmail = result.Email,
                    FromDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                    ToId = result.Id,
                    ToEmail = result.Email,
                    ToDisplay = string.Format("{0} {1}", result.FirstName, result.LastName),
                    Subject = string.Format("{0} {1}", issuer.Name, Constants.MsgConfirmNewUserSubject),
                    HtmlContent = EFCoreConstants.TemplateConfirmNewUser(issuer, result, url)
                });
            }

            return Ok(Mapper.Map<UserV1>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult CreateV1NoConfirm([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.Email == model.Email).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetIdentityGUID();

            //ignore how these may be set in model...
            model.HumanBeing = false;
            model.TwoFactorEnabled = false;
            model.EmailConfirmed = false;
            model.PasswordConfirmed = false;
            model.PhoneNumberConfirmed = false;

            if (!new ValidationHelper().ValidateEmail(model.Email).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{model.Email}");
                return BadRequest(ModelState);
            }

            var result = UoW.Users.Create(Mapper.Map<tbl_Users>(model));

            UoW.Commit();

            return Ok(Mapper.Map<UserV1>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

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

            user.ActorId = GetIdentityGUID();

            UoW.Users.Delete(user);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult DeleteRefreshV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
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

        [Route("v1/{userValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string userValue)
        {
            Guid userID;
            tbl_Users user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = UoW.Users.Get(x => x.Id == userID)
                    .SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.Email == userValue)
                    .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserV1>(user));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<UserV1>
                {
                    Data = Mapper.Map<IEnumerable<UserV1>>(
                        UoW.Users.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Users>, IQueryable<tbl_Users>>>>(
                                model.ToExpression<tbl_Users>()),
                            new List<Expression<Func<tbl_Users, object>>>() { x => x.tbl_UserLogins, x => x.tbl_UserRoles })),

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

        [Route("v1/{userID:guid}/claims"), HttpGet]
        public IActionResult GetClaimsV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var claims = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<ClaimV1>>(claims));
        }

        [Route("v1/{userID:guid}/audiences"), HttpGet]
        public IActionResult GetAudiencesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var audiences = UoW.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == userID))).ToLambda());

            return Ok(Mapper.Map<IEnumerable<AudienceV1>>(audiences));
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public IActionResult GetLoginsV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var logins = UoW.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<LoginV1>>(logins));
        }

        [Route("v1/{userID:guid}/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1([FromRoute] Guid userID)
        {
            var expr = new QueryExpression<tbl_Refreshes>()
                .Where(x => x.UserId == userID).ToLambda();

            if (!UoW.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var refreshes = UoW.Refreshes.Get(expr);

            return Ok(Mapper.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public IActionResult GetRolesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var roles = UoW.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleV1>>(roles));
        }

        [Route("v1/{userID:guid}/remove-from-claim/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult RemoveFromClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult RemoveFromLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = UoW.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult RemoveFromRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = UoW.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

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
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetIdentityGUID();

            if (!UoW.Users.IsPasswordSet(user))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetPasswordHash(user, null);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult SetPasswordV1([FromRoute] Guid userID, [FromBody] PasswordAddV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            user.ActorId = GetIdentityGUID();

            if (model.NewPassword != model.NewPasswordConfirm
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetPasswordHash(user, new ValidationHelper().PasswordHash(model.NewPassword));
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.PolicyForAdmins)]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.Get(x => x.Id == model.Id)
                .SingleOrDefault();

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

            model.ActorId = GetIdentityGUID();

            var result = UoW.Users.Update(Mapper.Map<tbl_Users>(model));

            UoW.Commit();

            return Ok(Mapper.Map<UserV1>(result));
        }
    }
}
