﻿using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.DataState.Extensions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
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
    [Route("claim")]
    public class ClaimController : BaseController
    {
        [Route("v1"), HttpPost]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult CreateV1([FromBody] ClaimV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.IssuerId == model.IssuerId && x.Type == model.Type).ToLambda())
                .Any())
            {
                ModelState.AddModelError(MessageType.ClaimAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Claim:{model.Type}");
                return BadRequest(ModelState);
            }

            var result = UoW.Claims.Create(Mapper.Map<tbl_Claim>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClaimV1>(result));
        }

        [Route("v1/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult DeleteV1([FromRoute] Guid claimID)
        {
            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (!claim.IsDeletable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claimID}");
                return BadRequest(ModelState);
            }

            UoW.Claims.Delete(claim);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{claimValue}"), HttpGet]
        public IActionResult GetV1([FromRoute] string claimValue)
        {
            Guid claimID;
            tbl_Claim claim = null;

            if (Guid.TryParse(claimValue, out claimID))
                claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Id == claimID).ToLambda())
                    .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"claimID: { claimValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClaimV1>(claim));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetV1([FromBody] DataStateV1 state)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new DataStateV1Result<ClaimV1>
                {
                    Data = Mapper.Map<IEnumerable<ClaimV1>>(
                        UoW.Claims.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claim>, IQueryable<tbl_Claim>>>>(
                                QueryExpressionFactory.GetQueryExpression<tbl_Claim>().ApplyState(state)),
                                    new List<Expression<Func<tbl_Claim, object>>>()
                                    {
                                        x => x.Issuer,
                                    })),

                    Total = UoW.Claims.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claim>, IQueryable<tbl_Claim>>>>(
                            QueryExpressionFactory.GetQueryExpression<tbl_Claim>().ApplyPredicate(state)))
                };

                return Ok(result);
            }
            catch (QueryExpressionException ex)
            {
                ModelState.AddModelError(MessageType.ParseError.ToString(), ex.ToString());
                return BadRequest(ModelState);
            }
        }

        [Route("v1"), HttpPut]
        [Authorize(Policy = DefaultConstants.OAuth2ROPGrants)]
        [Authorize(Roles = DefaultConstants.RoleForAdmins_Identity)]
        public IActionResult UpdateV1([FromBody] ClaimV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = UoW.Claims.GetAsNoTracking(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{model.Id}");
                return NotFound(ModelState);
            }
            else if (claim.IsDeletable
                && claim.IsDeletable != model.IsDeletable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claim.Id}");
                return BadRequest(ModelState);
            }

            var result = UoW.Claims.Update(Mapper.Map<tbl_Claim>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClaimV1>(result));
        }
    }
}
