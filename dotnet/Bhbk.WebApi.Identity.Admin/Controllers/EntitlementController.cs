using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Exceptions;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("entitlements")]
    public class EntitlementController : BaseController
    {
        // ── Shared lookups ──────────────────────────────────────────────────────

        [Route("v1/types"), HttpGet]
        public IActionResult GetTypesV1()
        {
            var types = uow.EntitlementTypes.Get(QueryExpressionFactory.GetQueryExpression<tbl_EntitlementType>()
                .Where(x => x.IsEnabled).ToLambda())
                .OrderBy(x => x.SortOrder);

            return Ok(map.Map<IEnumerable<EntitlementTypeV1>>(types));
        }

        [Route("v1/scopes"), HttpGet]
        public IActionResult GetScopesV1()
        {
            var scopes = uow.EntitlementScopes.Get(QueryExpressionFactory.GetQueryExpression<tbl_EntitlementScope>()
                .Where(x => x.IsEnabled).ToLambda())
                .OrderBy(x => x.SortOrder);

            return Ok(map.Map<IEnumerable<EntitlementScopeV1>>(scopes));
        }

        // ── User entitlements ───────────────────────────────────────────────────

        [Route("v1/users/page"), HttpPost]
        public IActionResult GetUserEntitlementsV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<UserEntitlementV1>
                {
                    Data = map.Map<IEnumerable<UserEntitlementV1>>(
                        uow.UserEntitlements.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_UserEntitlement>, IQueryable<tbl_UserEntitlement>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_UserEntitlement>().ApplyState(state)),
                                    new List<Expression<Func<tbl_UserEntitlement, object>>>()
                                    {
                                        x => x.User,
                                        x => x.EntitlementType,
                                        x => x.EntitlementScope,
                                        x => x.Issuer,
                                        x => x.Audience,
                                    })),

                    Total = uow.UserEntitlements.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_UserEntitlement>, IQueryable<tbl_UserEntitlement>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_UserEntitlement>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/users/{entitlementID:guid}"), HttpGet]
        public IActionResult GetUserEntitlementV1([FromRoute] Guid entitlementID)
        {
            var entitlement = uow.UserEntitlements.Get(
                QueryExpressionFactory.GetQueryExpression<tbl_UserEntitlement>()
                    .Where(x => x.Id == entitlementID).ToLambda(),
                new List<Expression<Func<tbl_UserEntitlement, object>>>()
                {
                    x => x.User,
                    x => x.EntitlementType,
                    x => x.EntitlementScope,
                    x => x.Issuer,
                    x => x.Audience,
                })
                .SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.UserEntitlementNotFound.ToString(), $"Entitlement:{entitlementID}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserEntitlementV1>(entitlement));
        }

        [Route("v1/users/user/{userID:guid}"), HttpGet]
        public IActionResult GetByUserV1([FromRoute] Guid userID)
        {
            var user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{userID}");
                return NotFound(ModelState);
            }

            var entitlements = uow.UserEntitlements.Get(
                QueryExpressionFactory.GetQueryExpression<tbl_UserEntitlement>()
                    .Where(x => x.UserId == userID).ToLambda(),
                new List<Expression<Func<tbl_UserEntitlement, object>>>()
                {
                    x => x.User,
                    x => x.EntitlementType,
                    x => x.EntitlementScope,
                    x => x.Issuer,
                    x => x.Audience,
                });

            return Ok(map.Map<IEnumerable<UserEntitlementV1>>(entitlements));
        }

        [Route("v1/users/me"), HttpGet]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        public IActionResult GetMyEntitlementsV1()
        {
            var userId = GetIdentityGUID();

            var entitlements = uow.UserEntitlements.Get(
                QueryExpressionFactory.GetQueryExpression<tbl_UserEntitlement>()
                    .Where(x => x.UserId == userId && x.IsEnabled).ToLambda(),
                new List<Expression<Func<tbl_UserEntitlement, object>>>()
                {
                    x => x.User,
                    x => x.EntitlementType,
                    x => x.EntitlementScope,
                    x => x.Issuer,
                    x => x.Audience,
                });

            return Ok(map.Map<IEnumerable<UserEntitlementV1>>(entitlements));
        }

        [Route("v1/users"), HttpPost]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult CreateUserEntitlementV1([FromBody] UserEntitlementV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == model.UserId).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{model.UserId}");
                return NotFound(ModelState);
            }

            var entitlementType = uow.EntitlementTypes.Get(x => x.Id == model.EntitlementTypeId).SingleOrDefault();

            if (entitlementType == null)
            {
                ModelState.AddModelError(MessageType.EntitlementTypeNotFound.ToString(), $"EntitlementType:{model.EntitlementTypeId}");
                return NotFound(ModelState);
            }

            var entitlementScope = uow.EntitlementScopes.Get(x => x.Id == model.EntitlementScopeId).SingleOrDefault();

            if (entitlementScope == null)
            {
                ModelState.AddModelError(MessageType.EntitlementScopeNotFound.ToString(), $"EntitlementScope:{model.EntitlementScopeId}");
                return NotFound(ModelState);
            }

            /* scope constraint validation */
            if (entitlementScope.Name == "Global")
            {
                if (model.IssuerId.HasValue || model.AudienceId.HasValue)
                {
                    ModelState.AddModelError(MessageType.UserEntitlementInvalid.ToString(), "Global scope must not have IssuerId or AudienceId");
                    return BadRequest(ModelState);
                }
            }
            else if (entitlementScope.Name == "Issuer")
            {
                if (!model.IssuerId.HasValue || model.AudienceId.HasValue)
                {
                    ModelState.AddModelError(MessageType.UserEntitlementInvalid.ToString(), "Issuer scope requires IssuerId and must not have AudienceId");
                    return BadRequest(ModelState);
                }
            }
            else if (entitlementScope.Name == "Audience")
            {
                if (!model.IssuerId.HasValue || !model.AudienceId.HasValue)
                {
                    ModelState.AddModelError(MessageType.UserEntitlementInvalid.ToString(), "Audience scope requires both IssuerId and AudienceId");
                    return BadRequest(ModelState);
                }
            }

            if (model.IssuerId.HasValue)
            {
                var issuer = uow.Issuers.Get(x => x.Id == model.IssuerId.Value).SingleOrDefault();

                if (issuer == null)
                {
                    ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                    return NotFound(ModelState);
                }
            }

            if (model.AudienceId.HasValue)
            {
                var audience = uow.Audiences.Get(x => x.Id == model.AudienceId.Value).SingleOrDefault();

                if (audience == null)
                {
                    ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{model.AudienceId}");
                    return NotFound(ModelState);
                }
            }

            /* check for duplicate */
            if (uow.UserEntitlements.Get(x =>
                x.UserId == model.UserId
                && x.EntitlementTypeId == model.EntitlementTypeId
                && x.EntitlementScopeId == model.EntitlementScopeId
                && x.IssuerId == model.IssuerId
                && x.AudienceId == model.AudienceId).Any())
            {
                ModelState.AddModelError(MessageType.UserEntitlementAlreadyExists.ToString(), $"Entitlement for User:{model.UserId}");
                return BadRequest(ModelState);
            }

            var entity = map.Map<tbl_UserEntitlement>(model);

            var entitlement = uow.UserEntitlements.Post(entity);
            uow.Commit();

            return Ok(map.Map<UserEntitlementV1>(entitlement));
        }

        [Route("v1/users"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult UpdateUserEntitlementV1([FromBody] UserEntitlementV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entitlement = uow.UserEntitlements.Get(x => x.Id == model.Id).SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.UserEntitlementNotFound.ToString(), $"Entitlement:{model.Id}");
                return NotFound(ModelState);
            }

            if (!entitlement.IsDeletable
                && entitlement.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.UserEntitlementImmutable.ToString(), $"Entitlement:{entitlement.Id}");
                return BadRequest(ModelState);
            }

            entitlement.IsEnabled = model.IsEnabled;
            entitlement.IsDeletable = model.IsDeletable;

            uow.Commit();

            return Ok(map.Map<UserEntitlementV1>(entitlement));
        }

        [Route("v1/users/{entitlementID:guid}"), HttpDelete]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult DeleteUserEntitlementV1([FromRoute] Guid entitlementID)
        {
            var entitlement = uow.UserEntitlements.Get(x => x.Id == entitlementID).SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.UserEntitlementNotFound.ToString(), $"Entitlement:{entitlementID}");
                return NotFound(ModelState);
            }

            if (!entitlement.IsDeletable)
            {
                ModelState.AddModelError(MessageType.UserEntitlementImmutable.ToString(), $"Entitlement:{entitlementID}");
                return BadRequest(ModelState);
            }

            uow.UserEntitlements.Delete(entitlement);
            uow.Commit();

            return NoContent();
        }

        // ── Audience entitlements ───────────────────────────────────────────────

        [Route("v1/audiences/page"), HttpPost]
        public IActionResult GetAudienceEntitlementsV1([FromBody] PagerState state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PagerStateResult<AudienceEntitlementV1>
                {
                    Data = map.Map<IEnumerable<AudienceEntitlementV1>>(
                        uow.AudienceEntitlements.Get(
                            map.MapExpression<Expression<Func<IQueryable<tbl_AudienceEntitlement>, IQueryable<tbl_AudienceEntitlement>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_AudienceEntitlement>().ApplyState(state)),
                                    new List<Expression<Func<tbl_AudienceEntitlement, object>>>()
                                    {
                                        x => x.Audience,
                                        x => x.EntitlementType,
                                        x => x.EntitlementScope,
                                        x => x.Issuer,
                                    })),

                    Total = uow.AudienceEntitlements.Count(
                        map.MapExpression<Expression<Func<IQueryable<tbl_AudienceEntitlement>, IQueryable<tbl_AudienceEntitlement>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_AudienceEntitlement>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1/audiences/{entitlementID:guid}"), HttpGet]
        public IActionResult GetAudienceEntitlementV1([FromRoute] Guid entitlementID)
        {
            var entitlement = uow.AudienceEntitlements.Get(
                QueryExpressionFactory.GetQueryExpression<tbl_AudienceEntitlement>()
                    .Where(x => x.Id == entitlementID).ToLambda(),
                new List<Expression<Func<tbl_AudienceEntitlement, object>>>()
                {
                    x => x.Audience,
                    x => x.EntitlementType,
                    x => x.EntitlementScope,
                    x => x.Issuer,
                })
                .SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementNotFound.ToString(), $"AudienceEntitlement:{entitlementID}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<AudienceEntitlementV1>(entitlement));
        }

        [Route("v1/audiences/audience/{audienceID:guid}"), HttpGet]
        public IActionResult GetByAudienceV1([FromRoute] Guid audienceID)
        {
            var audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{audienceID}");
                return NotFound(ModelState);
            }

            var entitlements = uow.AudienceEntitlements.Get(
                QueryExpressionFactory.GetQueryExpression<tbl_AudienceEntitlement>()
                    .Where(x => x.AudienceId == audienceID).ToLambda(),
                new List<Expression<Func<tbl_AudienceEntitlement, object>>>()
                {
                    x => x.Audience,
                    x => x.EntitlementType,
                    x => x.EntitlementScope,
                    x => x.Issuer,
                });

            return Ok(map.Map<IEnumerable<AudienceEntitlementV1>>(entitlements));
        }

        [Route("v1/audiences"), HttpPost]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult CreateAudienceEntitlementV1([FromBody] AudienceEntitlementV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = uow.Audiences.Get(x => x.Id == model.AudienceId).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{model.AudienceId}");
                return NotFound(ModelState);
            }

            var entitlementType = uow.EntitlementTypes.Get(x => x.Id == model.EntitlementTypeId).SingleOrDefault();

            if (entitlementType == null)
            {
                ModelState.AddModelError(MessageType.EntitlementTypeNotFound.ToString(), $"EntitlementType:{model.EntitlementTypeId}");
                return NotFound(ModelState);
            }

            var entitlementScope = uow.EntitlementScopes.Get(x => x.Id == model.EntitlementScopeId).SingleOrDefault();

            if (entitlementScope == null)
            {
                ModelState.AddModelError(MessageType.EntitlementScopeNotFound.ToString(), $"EntitlementScope:{model.EntitlementScopeId}");
                return NotFound(ModelState);
            }

            /* scope constraint validation */
            if (entitlementScope.Name == "Global")
            {
                if (model.IssuerId.HasValue)
                {
                    ModelState.AddModelError(MessageType.AudienceEntitlementInvalid.ToString(), "Global scope must not have IssuerId");
                    return BadRequest(ModelState);
                }
            }
            else if (entitlementScope.Name == "Issuer")
            {
                if (!model.IssuerId.HasValue)
                {
                    ModelState.AddModelError(MessageType.AudienceEntitlementInvalid.ToString(), "Issuer scope requires IssuerId");
                    return BadRequest(ModelState);
                }
            }

            if (model.IssuerId.HasValue)
            {
                var issuer = uow.Issuers.Get(x => x.Id == model.IssuerId.Value).SingleOrDefault();

                if (issuer == null)
                {
                    ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{model.IssuerId}");
                    return NotFound(ModelState);
                }
            }

            /* check for duplicate */
            if (uow.AudienceEntitlements.Get(x =>
                x.AudienceId == model.AudienceId
                && x.EntitlementTypeId == model.EntitlementTypeId
                && x.EntitlementScopeId == model.EntitlementScopeId
                && x.IssuerId == model.IssuerId).Any())
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementAlreadyExists.ToString(), $"AudienceEntitlement for Audience:{model.AudienceId}");
                return BadRequest(ModelState);
            }

            var entity = map.Map<tbl_AudienceEntitlement>(model);

            var entitlement = uow.AudienceEntitlements.Post(entity);
            uow.Commit();

            return Ok(map.Map<AudienceEntitlementV1>(entitlement));
        }

        [Route("v1/audiences"), HttpPut]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult UpdateAudienceEntitlementV1([FromBody] AudienceEntitlementV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entitlement = uow.AudienceEntitlements.Get(x => x.Id == model.Id).SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementNotFound.ToString(), $"AudienceEntitlement:{model.Id}");
                return NotFound(ModelState);
            }

            if (!entitlement.IsDeletable
                && entitlement.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementImmutable.ToString(), $"AudienceEntitlement:{entitlement.Id}");
                return BadRequest(ModelState);
            }

            entitlement.IsEnabled = model.IsEnabled;
            entitlement.IsDeletable = model.IsDeletable;

            uow.Commit();

            return Ok(map.Map<AudienceEntitlementV1>(entitlement));
        }

        [Route("v1/audiences/{entitlementID:guid}"), HttpDelete]
        [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
        [Authorize(Policy = PolicyConstants.EntitlementAdminPolicy)]
        public IActionResult DeleteAudienceEntitlementV1([FromRoute] Guid entitlementID)
        {
            var entitlement = uow.AudienceEntitlements.Get(x => x.Id == entitlementID).SingleOrDefault();

            if (entitlement == null)
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementNotFound.ToString(), $"AudienceEntitlement:{entitlementID}");
                return NotFound(ModelState);
            }

            if (!entitlement.IsDeletable)
            {
                ModelState.AddModelError(MessageType.AudienceEntitlementImmutable.ToString(), $"AudienceEntitlement:{entitlementID}");
                return BadRequest(ModelState);
            }

            uow.AudienceEntitlements.Delete(entitlement);
            uow.Commit();

            return NoContent();
        }
    }
}
