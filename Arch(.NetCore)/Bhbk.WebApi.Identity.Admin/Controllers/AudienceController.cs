using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    public class AudienceController : BaseController
    {
        [Route("v1/{audienceID:guid}/add-to-role/{roleID:guid}"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult AddToRoleV1([FromRoute] Guid audienceID, [FromRoute] Guid roleID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var role = uow.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }

            if (!uow.Audiences.IsInRole(audience, role))
            {
                uow.Audiences.AddRole(
                    new tbl_AudienceRole()
                    {
                        AudienceId = audience.Id,
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
        public IActionResult CreateV1([FromBody] AudienceV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (uow.Audiences.Get(x => x.IssuerId == model.IssuerId
                && x.Name == model.Name).Any())
            {
                ModelState.AddModelError(MessageType.AudienceAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Audience:{model.Name}");
                return BadRequest(ModelState);
            }

            var result = uow.Audiences.Create(map.Map<tbl_Audience>(model));

            uow.Commit();

            return Ok(map.Map<AudienceV1>(result));
        }

        [Route("v1/{audienceID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }
            
            if (!audience.IsDeletable)
            {
                ModelState.AddModelError(MessageType.AudienceImmutable.ToString(), $"Audience:{audienceID}");
                return BadRequest(ModelState);
            }

            uow.Audiences.Delete(audience);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/refresh"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteRefreshesV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.AudienceId == audienceID).ToLambda());

            uow.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/refresh/{refreshID}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteRefreshV1([FromRoute] Guid audienceID, [FromRoute] Guid refreshID)
        {
            var expr = QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.AudienceId == audienceID && x.Id == refreshID).ToLambda();

            if (!uow.Refreshes.Exists(expr))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshID}");
                return NotFound(ModelState);
            }

            uow.Refreshes.Delete(expr);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{audienceValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string audienceValue)
        {
            Guid audienceID;
            LambdaExpression expr = null;
            tbl_Audience audience = null;

            if (Guid.TryParse(audienceValue, out audienceID))
                expr = QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.Id == audienceID).ToLambda();
            else
                expr = QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.Name == audienceValue).ToLambda();

            audience = uow.Audiences.Get(expr,
                new List<Expression<Func<tbl_Audience, object>>>()
                {
                    x => x.tbl_AudienceRoles,
                    x => x.tbl_Roles,
                    x => x.tbl_Urls,
                })
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceValue}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<AudienceV1>(audience));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<AudienceV1>
                {
                    Data = map.Map<IEnumerable<AudienceV1>>(
                        uow.Audiences.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_Audience>, IQueryable<tbl_Audience>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Audience>().ApplyState(state)),
                                    new List<Expression<Func<tbl_Audience, object>>>() 
                                    {
                                        x => x.tbl_AudienceRoles,
                                        x => x.tbl_Roles,
                                        x => x.tbl_Urls,
                                    })),

                    Total = uow.Audiences.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_Audience>, IQueryable<tbl_Audience>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Audience>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/{audienceID:guid}/refreshes"), HttpGet]
        public IActionResult GetRefreshesV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var refreshes = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(map.Map<IEnumerable<RefreshV1>>(refreshes));
        }

        [Route("v1/{audienceID:guid}/roles"), HttpGet]
        public IActionResult GetRolesV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var roles = uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(map.Map<IEnumerable<RoleV1>>(roles));
        }

        [Route("v1/{audienceID:guid}/urls"), HttpGet]
        public IActionResult GetUrlsV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var urls = uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Url>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            return Ok(map.Map<IEnumerable<UrlV1>>(urls));
        }

        [Route("v1/{audienceID:guid}/remove-from-role/{roleID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemoveFromRoleV1([FromRoute] Guid audienceID, [FromRoute] Guid roleID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var role = uow.Roles.Get(x => x.Id == roleID)
                .SingleOrDefault();

            if (role == null)
            {
                ModelState.AddModelError(MessageType.RoleNotFound.ToString(), $"Role:{roleID}");
                return NotFound(ModelState);
            }
            
            if(uow.Audiences.IsInRole(audience, role))
            {
                uow.Audiences.RemoveRole(
                    new tbl_AudienceRole()
                    {
                        AudienceId = audience.Id,
                        RoleId = role.Id,
                    });
                uow.Commit();
            }

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/remove-password"), HttpGet]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult RemovePasswordV1([FromRoute] Guid audienceID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            if (!uow.Audiences.IsPasswordSet(audience))
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"No password set for audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            uow.Audiences.SetPassword(audience, null);
            uow.Commit();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}/set-password"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult SetPasswordV1([FromRoute] Guid audienceID, [FromBody] PasswordAddV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = uow.Audiences.Get(x => x.Id == audienceID)
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            if (model.NewPassword != model.NewPasswordConfirm
                || !new ValidationHelper().ValidatePassword(model.NewPassword).Succeeded)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Bad password for audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            uow.Audiences.SetPassword(audience, model.NewPassword);
            uow.Commit();

            return NoContent();
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult UpdateV1([FromBody] AudienceV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = uow.Audiences.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{model.Id}");
                return NotFound(ModelState);
            }
            else if (audience.IsDeletable
                && audience.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.AudienceImmutable.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var result = uow.Audiences.Update(map.Map<tbl_Audience>(model));

            uow.Commit();

            return Ok(map.Map<AudienceV1>(result));
        }
    }
}