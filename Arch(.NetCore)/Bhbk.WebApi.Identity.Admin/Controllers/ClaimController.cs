using AutoMapper.Extensions.ExpressionMapping;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Admin;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
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
        private ClaimProvider _provider;

        public ClaimController(IConfiguration conf, IContextService instance)
        {
            _provider = new ClaimProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult CreateClaimV1([FromBody] ClaimCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.IssuerId == model.IssuerId && x.Type == model.Type).ToLambda())
                .Any())
            {
                ModelState.AddModelError(MessageType.ClaimAlreadyExists.ToString(), $"Issuer:{model.IssuerId} Claim:{model.Type}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = UoW.Claims.Create(Mapper.Map<tbl_Claims>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClaimModel>(result));
        }

        [Route("v1/{claimID:guid}"), HttpDelete]
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult DeleteClaimV1([FromRoute] Guid claimID)
        {
            var claim = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == claimID).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{claimID}");
                return NotFound(ModelState);
            }

            if (claim.Immutable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claimID}");
                return BadRequest(ModelState);
            }

            claim.ActorId = GetUserGUID();

            UoW.Claims.Delete(claim);
            UoW.Commit();

            return NoContent();
        }

        [Route("v1/{claimValue}"), HttpGet]
        public IActionResult GetClaimV1([FromRoute] string claimValue)
        {
            Guid claimID;
            tbl_Claims claim = null;

            if (Guid.TryParse(claimValue, out claimID))
                claim = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                    .Where(x => x.Id == claimID).ToLambda())
                    .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"claimID: { claimValue }");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<ClaimModel>(claim));
        }

        [Route("v1/page"), HttpPost]
        public IActionResult GetClaimsV1([FromBody] PageStateTypeC model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = new PageStateTypeCResult<ClaimModel>
                {
                    Data = Mapper.Map<IEnumerable<ClaimModel>>(
                        UoW.Claims.Get(
                            Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claims>, IQueryable<tbl_Claims>>>>(
                                model.ToExpression<tbl_Claims>()))),

                    Total = UoW.Claims.Count(
                        Mapper.MapExpression<Expression<Func<IQueryable<tbl_Claims>, IQueryable<tbl_Claims>>>>(
                            model.ToPredicateExpression<tbl_Claims>()))
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
        [Authorize(Policy = "AdministratorsPolicy")]
        public IActionResult UpdateClaimV1([FromBody] ClaimModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = UoW.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Id == model.Id).ToLambda())
                .SingleOrDefault();

            if (claim == null)
            {
                ModelState.AddModelError(MessageType.ClaimNotFound.ToString(), $"Claim:{model.Id}");
                return NotFound(ModelState);
            }
            else if (claim.Immutable
                && claim.Immutable != model.Immutable)
            {
                ModelState.AddModelError(MessageType.ClaimImmutable.ToString(), $"Claim:{claim.Id}");
                return BadRequest(ModelState);
            }

            model.ActorId = GetUserGUID();

            var result = UoW.Claims.Update(Mapper.Map<tbl_Claims>(model));

            UoW.Commit();

            return Ok(Mapper.Map<ClaimModel>(result));
        }
    }
}
