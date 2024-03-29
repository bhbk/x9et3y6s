﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Templates;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        [Route("v1/{userID:guid}/add-to-claim/{claimID:guid}"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult AddToClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (!uow.Users.IsInClaim(user, claim))
            {
                uow.Users.AddClaim(
                    new tbl_UserClaim()
                    {
                        UserId = user.Id,
                        ClaimId = claim.Id,
                        IsDeletable = true,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-login/{loginID:guid}"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult AddToLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = uow.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            if (!uow.Users.IsInLogin(user, login))
            {
                uow.Users.AddLogin(
                    new tbl_UserLogin()
                    {
                        UserId = user.Id,
                        LoginId = login.Id,
                        IsDeletable = true,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-role/{roleID:guid}"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult AddToRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = uow.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            if (!uow.Users.IsInRole(user, role))
            {
                uow.Users.AddRole(
                    new tbl_UserRole()
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        IsDeletable = true,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public async ValueTask<IActionResult> CreateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (uow.Users.Get(x => x.UserName == model.UserName).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.UserName}");
                return BadRequest(ModelState);
            }

            var issuer = uow.Issuers.Get(x => x.Id == model.IssuerId)
                .SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                return NotFound(ModelState);
            }

            //ignore how these may be set in model...
            model.IsHumanBeing = true;
            model.EmailConfirmed = false;
            model.PhoneNumberConfirmed = false;
            model.PasswordConfirmed = false;

            if (!new ValidationHelper().ValidateEmail(model.UserName).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{model.UserName}");
                return BadRequest(ModelState);
            }

            var result = uow.Users.Create(map.Map<tbl_User>(model));

            uow.Commit();

            if (uow.InstanceType == InstanceContext.DeployedOrLocal
                || uow.InstanceType == InstanceContext.End2EndTest)
            {
                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.TotpExpire).Single();

                var code = HttpUtility.UrlEncode(new PasswordTokenFactory(uow.InstanceType.ToString())
                    .Generate(result.UserName, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), result.Id.ToString(), result.SecurityStamp));

                var url = UrlFactory.GenerateConfirmEmailV1(conf, result.Id.ToString(), code);
                var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

                await alert.Enqueue_EmailV1(
                    new EmailV1()
                    {
                        FromEmail = result.EmailAddress,
                        FromDisplay = $"{result.FirstName} {result.LastName}",
                        ToEmail = result.EmailAddress,
                        ToDisplay = $"{result.FirstName} {result.LastName}",
                        Subject = $"{issuer.Name} {MessageConstants.ConfirmNewUserSubject}",
                        Body = Email.ConfirmNewUser(map.Map<IssuerV1>(issuer), map.Map<UserV1>(result), url)
                    });
            }

            return Ok(map.Map<UserV1>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult CreateV1NoConfirm([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (uow.Users.Get(x => x.UserName == model.UserName).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.UserName}");
                return BadRequest(ModelState);
            }

            //ignore how these may be set in model...
            model.IsHumanBeing = false;
            model.EmailConfirmed = false;
            model.PhoneNumberConfirmed = false;
            model.PasswordConfirmed = false;

            if (!new ValidationHelper().ValidateEmail(model.UserName).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{model.UserName}");
                return BadRequest(ModelState);
            }

            var result = uow.Users.Create(map.Map<tbl_User>(model));

            uow.Commit();

            return Ok(map.Map<UserV1>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!user.IsDeletable)
            {
                ModelState.AddModelError(MessageType.UserImmutable.ToString(), $"User:{userID}");
                return BadRequest(ModelState);
            }

            uow.Users.Delete(user);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == userID).ToLambda());

            uow.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh/{refreshID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteRefreshV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == userID && x.Id == refreshID).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{userID}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(expr);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{userValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string userValue)
        {
            Guid userID;
            tbl_User user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = uow.Users.Get(x => x.Id == userID)
                    .SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == userValue)
                    .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserV1>(user));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<UserV1>
                {
                    Data = map.Map<IEnumerable<UserV1>>(
                        uow.Users.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_User>, IQueryable<tbl_User>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_User>().ApplyState(state)),
                                    new List<Expression<Func<tbl_User, object>>>() 
                                    { 
                                        x => x.tbl_AuthActivities,
                                        x => x.tbl_UserClaims,
                                        x => x.tbl_UserLogins, 
                                        x => x.tbl_UserRoles,
                                    })),

                    Total = uow.Users.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_User>, IQueryable<tbl_User>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_User>().ApplyPredicate(state)))
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
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var claims = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.tbl_UserClaims.Any(y => y.UserId == userID)).ToLambda());

            return Ok(map.Map<IEnumerable<ClaimV1>>(claims));
        }

        [Route("v1/{userID:guid}/audiences"), HttpGet]
        public IActionResult GetAudiencesV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var audiences = uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == userID))).ToLambda());

            return Ok(map.Map<IEnumerable<AudienceV1>>(audiences));
        }

        [Route("v1/{userID:guid}/logins"), HttpGet]
        public IActionResult GetLoginsV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var logins = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == userID)).ToLambda());

            return Ok(map.Map<IEnumerable<LoginV1>>(logins));
        }

        [Route("v1/{userID:guid}/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1([FromRoute] Guid userID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == userID).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var refreshes = uow.Refreshes.Get(expr);

            return Ok(map.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/{userID:guid}/roles"), HttpGet]
        public IActionResult GetRolesV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var roles = uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == userID)).ToLambda());

            return Ok(map.Map<IEnumerable<RoleV1>>(roles));
        }

        [Route("v1/{userID:guid}/remove-from-claim/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemoveFromClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (uow.Users.IsInClaim(user, claim))
            {
                uow.Users.RemoveClaim(
                    new tbl_UserClaim()
                    {
                        UserId = user.Id,
                        ClaimId = claim.Id,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-login/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemoveFromLoginV1([FromRoute] Guid userID, [FromRoute] Guid loginID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var login = uow.Logins.Get(x => x.Id == loginID)
                .SingleOrDefault();

            if (login == null)
            {
                ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"Login:{loginID}");
                return NotFound(ModelState);
            }

            if (uow.Users.IsInLogin(user, login))
            {
                uow.Users.RemoveLogin(
                    new tbl_UserLogin()
                    {
                        UserId = user.Id,
                        LoginId = login.Id,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-role/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemoveFromRoleV1([FromRoute] Guid userID, [FromRoute] Guid roleID)
        {
            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var role = uow.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            if (uow.Users.IsInRole(user, role))
            {
                uow.Users.RemoveRole(
                    new tbl_UserRole()
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemovePasswordV1([FromRoute] Guid userID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (!uow.Users.IsPasswordSet(user))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"No password set for user:{user.Id}");
                return BadRequest(ModelState);
            }

            uow.Users.SetPassword(user, null);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult SetPasswordV1([FromRoute] Guid userID, [FromBody] PasswordAddV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            if (model.NewPassword != model.NewPasswordConfirm
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            uow.Users.SetPassword(user, model.NewPassword);
            uow.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.Id}");
                return NotFound(ModelState);
            }
            else if (user.IsDeletable
                && user.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.UserImmutable.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var result = uow.Users.Update(map.Map<tbl_User>(model));

            uow.Commit();

            return Ok(map.Map<UserV1>(result));
        }
    }
}
