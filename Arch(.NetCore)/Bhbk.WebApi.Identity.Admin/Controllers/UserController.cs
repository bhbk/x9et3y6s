using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_TBL;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Domain.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
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
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult AddToClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (!UoW.Users.IsInClaim(user, claim))
            {
                UoW.Users.AddClaim(
                    new tbl_UserClaim()
                    {
                        UserId = user.Id,
                        ClaimId = claim.Id,
                        IsDeletable = true,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-login/{loginID:guid}"), HttpGet]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (!UoW.Users.IsInLogin(user, login))
            {
                UoW.Users.AddLogin(
                    new tbl_UserLogin()
                    {
                        UserId = user.Id,
                        LoginId = login.Id,
                        IsDeletable = true,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/add-to-role/{roleID:guid}"), HttpGet]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (!UoW.Users.IsInRole(user, role))
            {
                UoW.Users.AddRole(
                    new tbl_UserRole()
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        IsDeletable = true,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public async ValueTask<IActionResult> CreateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.UserName == model.UserName).Any())
            {
                ModelState.AddModelError(MessageType.UserAlreadyExists.ToString(), $"User:{model.UserName}");
                return BadRequest(ModelState);
            }

            var issuer = UoW.Issuers.Get(x => x.Id == model.IssuerId)
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

            var result = UoW.Users.Create(Mapper.Map<tbl_User>(model));

            UoW.Commit();

            if (UoW.InstanceType == InstanceContext.DeployedOrLocal
                || UoW.InstanceType == InstanceContext.End2EndTest)
            {
                var expire = UoW.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.SettingTotpExpire).Single();

                var code = HttpUtility.UrlEncode(new PasswordTokenFactory(UoW.InstanceType.ToString())
                    .Generate(result.UserName, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), result.Id.ToString(), result.SecurityStamp));

                var url = UrlFactory.GenerateConfirmEmailV1(Conf, result.Id.ToString(), code);
                var alert = ControllerContext.HttpContext.RequestServices.GetRequiredService<IAlertService>();

                await alert.Enqueue_EmailV1(
                    new EmailV1()
                    {
                        FromEmail = result.EmailAddress,
                        FromDisplay = $"{result.FirstName} {result.LastName}",
                        ToEmail = result.EmailAddress,
                        ToDisplay = $"{result.FirstName} {result.LastName}",
                        Subject = $"{issuer.Name} {Constants.MsgConfirmNewUserSubject}",
                        Body = Templates.ConfirmNewUser(Mapper.Map<IssuerV1>(issuer), Mapper.Map<UserV1>(result), url)
                    });
            }

            return Ok(Mapper.Map<UserV1>(result));
        }

        [Route("v1/no-confirm"), HttpPost]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult CreateV1NoConfirm([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Users.Get(x => x.UserName == model.UserName).Any())
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

            var result = UoW.Users.Create(Mapper.Map<tbl_User>(model));

            UoW.Commit();

            return Ok(Mapper.Map<UserV1>(result));
        }

        [Route("v1/{userID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
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

            UoW.Users.Delete(user);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == userID).ToLambda());

            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/refresh/{refreshID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult DeleteRefreshV1([FromRoute] Guid userID, [FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
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
            tbl_User user = null;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(userValue, out userID))
                user = UoW.Users.Get(x => x.Id == userID)
                    .SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.UserName == userValue)
                    .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userValue}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<UserV1>(user));
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
                    Data = Mapper.Map<IEnumerable<UserV1>>(
                        UoW.Users.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_User>, IQueryable<tbl_User>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_User>().ApplyState(state)),
                            new List<Expression<Func<tbl_User, object>>>() { x => x.tbl_UserLogins, x => x.tbl_UserRoles })),

                    Total = UoW.Users.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_User>, IQueryable<tbl_User>>>>(
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
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var claims = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
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

            var audiences = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
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

            var logins = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<LoginV1>>(logins));
        }

        [Route("v1/{userID:guid}/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1([FromRoute] Guid userID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
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

            var roles = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.tbl_UserRoles.Any(y => y.UserId == userID)).ToLambda());

            return Ok(Mapper.Map<IEnumerable<RoleV1>>(roles));
        }

        [Route("v1/{userID:guid}/remove-from-claim/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult RemoveFromClaimV1([FromRoute] Guid userID, [FromRoute] Guid claimID)
        {
            var user = UoW.Users.Get(x => x.Id == userID)
                .SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (UoW.Users.IsInClaim(user, claim))
            {
                UoW.Users.RemoveClaim(
                    new tbl_UserClaim()
                    {
                        UserId = user.Id,
                        ClaimId = claim.Id,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-login/{loginID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (UoW.Users.IsInLogin(user, login))
            {
                UoW.Users.RemoveLogin(
                    new tbl_UserLogin()
                    {
                        UserId = user.Id,
                        LoginId = login.Id,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-from-role/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (UoW.Users.IsInRole(user, role))
            {
                UoW.Users.RemoveRole(
                    new tbl_UserRole()
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                    });
                UoW.Commit();
            }

            return NoContent();
        }

        [Route("v1/{userID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (!UoW.Users.IsPasswordSet(user))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"No password set for user:{user.Id}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetPassword(user, null);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{userID:guid}/set-password"), HttpPut]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
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

            if (model.NewPassword != model.NewPasswordConfirm
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"Bad password for user:{user.Id}");
                return BadRequest(ModelState);
            }

            UoW.Users.SetPassword(user, model.NewPassword);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = Constants.DefaultPolicyForHumans)]
        [Authorize(Roles = Constants.DefaultRoleForAdmin_Identity)]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = UoW.Users.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_User>()
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

            var result = UoW.Users.Update(Mapper.Map<tbl_User>(model));

            UoW.Commit();

            return Ok(Mapper.Map<UserV1>(result));
        }

        [Route("v1/{userID:guid}/verify"), HttpGet]
        public IActionResult VerifyV1([FromRoute] Guid userID)
        {
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var logins = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == user.Id)).ToLambda());

            switch (UoW.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        //check if login provider is local...
                        if (!logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                            return BadRequest(ModelState);
                        }
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        //check if login provider is local or test...
                        if (!logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                            && !logins.Where(x => x.Name.StartsWith(Constants.TestLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                            return BadRequest(ModelState);
                        }

                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            return NoContent();
        }
    }
}
